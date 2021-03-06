﻿#region Licence
/**
* Copyright © 2015-2018 OTTools <https://github.com/ottools/open-tibia>
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/
#endregion

#region Using Statements
using OpenTibia.Animation;
using OpenTibia.Client.Sprites;
using OpenTibia.Client.Things;
using OpenTibia.IO;
using OpenTibia.Utils;
using System;
using System.IO;
using System.Text;
#endregion

namespace OpenTibia.Obd
{
    public static class ObdEncoder
    {
        #region | Public Static Methods |

        public static byte[] Encode(ObjectData data, ObdVersion obdVersion)
        {
            if (obdVersion == ObdVersion.Version3)
            {
                return EncodeV3(data);
            }
            else if (obdVersion == ObdVersion.Version2)
            {
                return EncodeV2(data);
            }
            else if (obdVersion == ObdVersion.Version1)
            {
                return EncodeV1(data);
            }

            return null;
        }

        #endregion

        #region | Private Static Methods |

        private static byte[] EncodeV1(ObjectData data)
        {
            using (FlagsBinaryWriter writer = new FlagsBinaryWriter(new MemoryStream()))
            {
                // write client version
                writer.Write((ushort)DatFormat.Format_1010);

                // write category
                string category = string.Empty;
                switch (data.Category)
                {
                    case ThingCategory.Item:
                        category = "item";
                        break;

                    case ThingCategory.Outfit:
                        category = "outfit";
                        break;

                    case ThingCategory.Effect:
                        category = "effect";
                        break;

                    case ThingCategory.Missile:
                        category = "missile";
                        break;
                }

                writer.Write((ushort)category.Length);
                writer.Write(Encoding.UTF8.GetBytes(category));

                if (!ThingTypeSerializer.WriteProperties(data.ThingType, DatFormat.Format_1010, writer))
                {
                    return null;
                }

                FrameGroup group = data.GetFrameGroup(FrameGroupType.Default);

                writer.Write(group.Width);
                writer.Write(group.Height);

                if (group.Width > 1 || group.Height > 1)
                {
                    writer.Write(group.ExactSize);
                }

                writer.Write(group.Layers);
                writer.Write(group.PatternX);
                writer.Write(group.PatternY);
                writer.Write(group.PatternZ);
                writer.Write(group.Frames);

                Sprite[] sprites = data.Sprites[FrameGroupType.Default];
                for (int i = 0; i < sprites.Length; i++)
                {
                    Sprite sprite = sprites[i];
                    byte[] pixels = sprite.GetARGBPixels();
                    writer.Write((uint)sprite.ID);
                    writer.Write((uint)pixels.Length);
                    writer.Write(pixels);
                }

                return LZMACoder.Compress(((MemoryStream)writer.BaseStream).ToArray());
            }
        }

        private static byte[] EncodeV2(ObjectData data)
        {
            using (FlagsBinaryWriter writer = new FlagsBinaryWriter(new MemoryStream()))
            {
                // write obd version
                writer.Write((ushort)ObdVersion.Version2);

                // write client version
                writer.Write((ushort)DatFormat.Format_1050);

                // write category
                writer.Write((byte)data.Category);

                // skipping the texture patterns position.
                int patternsPosition = (int)writer.BaseStream.Position;
                writer.Seek(4, SeekOrigin.Current);

                if (!WriteProperties(data.ThingType, writer))
                {
                    return null;
                }

                // write the texture patterns position.
                int position = (int)writer.BaseStream.Position;
                writer.Seek(patternsPosition, SeekOrigin.Begin);
                writer.Write((uint)writer.BaseStream.Position);
                writer.Seek(position, SeekOrigin.Begin);

                FrameGroup group = data.GetFrameGroup(FrameGroupType.Default);

                writer.Write(group.Width);
                writer.Write(group.Height);

                if (group.Width > 1 || group.Height > 1)
                {
                    writer.Write(group.ExactSize);
                }

                writer.Write(group.Layers);
                writer.Write(group.PatternX);
                writer.Write(group.PatternY);
                writer.Write(group.PatternZ);
                writer.Write(group.Frames);

                if (group.IsAnimation)
                {
                    writer.Write((byte)group.AnimationMode);
                    writer.Write(group.LoopCount);
                    writer.Write(group.StartFrame);

                    for (int i = 0; i < group.Frames; i++)
                    {
                        writer.Write((uint)group.FrameDurations[i].Minimum);
                        writer.Write((uint)group.FrameDurations[i].Maximum);
                    }
                }

                Sprite[] sprites = data.Sprites[FrameGroupType.Default];
                for (int i = 0; i < sprites.Length; i++)
                {
                    Sprite sprite = sprites[i];
                    byte[] pixels = sprite.GetARGBPixels();
                    writer.Write(sprite.ID);
                    writer.Write(pixels);
                }

                return LZMACoder.Compress(((MemoryStream)writer.BaseStream).ToArray());
            }
        }

        private static byte[] EncodeV3(ObjectData data)
        {
            throw new NotImplementedException();
        }

        public static bool WriteProperties(ThingType thing, FlagsBinaryWriter output)
        {
            if (thing.Category == ThingCategory.Item)
            {
                if (thing.StackOrder == StackOrder.Ground)
                {
                    output.Write(ObdFlags.Ground);
                    output.Write(thing.GroundSpeed);
                }
                else if (thing.StackOrder == StackOrder.Border)
                {
                    output.Write(ObdFlags.GroundBorder);
                }
                else if (thing.StackOrder == StackOrder.Bottom)
                {
                    output.Write(ObdFlags.OnBottom);
                }
                else if (thing.StackOrder == StackOrder.Top)
                {
                    output.Write(ObdFlags.OnTop);
                }

                if (thing.IsContainer)
                {
                    output.Write(ObdFlags.Container);
                }

                if (thing.Stackable)
                {
                    output.Write(ObdFlags.Stackable);
                }

                if (thing.ForceUse)
                {
                    output.Write(ObdFlags.ForceUse);
                }

                if (thing.MultiUse)
                {
                    output.Write(ObdFlags.MultiUse);
                }

                if (thing.Writable)
                {
                    output.Write(ObdFlags.Writable);
                    output.Write(thing.MaxTextLength);
                }

                if (thing.WritableOnce)
                {
                    output.Write(ObdFlags.WritableOnce);
                    output.Write(thing.MaxTextLength);
                }

                if (thing.IsFluidContainer)
                {
                    output.Write(ObdFlags.FluidContainer);
                }

                if (thing.IsFluid)
                {
                    output.Write(ObdFlags.Fluid);
                }

                if (thing.Unpassable)
                {
                    output.Write(ObdFlags.IsUnpassable);
                }

                if (thing.Unmovable)
                {
                    output.Write(ObdFlags.IsUnmovable);
                }

                if (thing.BlockMissiles)
                {
                    output.Write(ObdFlags.BlockMissiles);
                }

                if (thing.BlockPathfinder)
                {
                    output.Write(ObdFlags.BlockPathfinder);
                }

                if (thing.NoMoveAnimation)
                {
                    output.Write(ObdFlags.NoMoveAnimation);
                }

                if (thing.Pickupable)
                {
                    output.Write(ObdFlags.Pickupable);
                }

                if (thing.Hangable)
                {
                    output.Write(ObdFlags.Hangable);
                }

                if (thing.HookSouth)
                {
                    output.Write(ObdFlags.HookSouth);
                }

                if (thing.HookEast)
                {
                    output.Write(ObdFlags.HookEast);
                }

                if (thing.Rotatable)
                {
                    output.Write(ObdFlags.Rotatable);
                }

                if (thing.DontHide)
                {
                    output.Write(ObdFlags.DontHide);
                }

                if (thing.Translucent)
                {
                    output.Write(ObdFlags.Translucent);
                }

                if (thing.HasElevation)
                {
                    output.Write(ObdFlags.HasElevation);
                    output.Write(thing.Elevation);
                }

                if (thing.LyingObject)
                {
                    output.Write(ObdFlags.LyingObject);
                }

                if (thing.Minimap)
                {
                    output.Write(ObdFlags.Minimap);
                    output.Write(thing.MinimapColor);
                }

                if (thing.IsLensHelp)
                {
                    output.Write(ObdFlags.LensHelp);
                    output.Write(thing.LensHelp);
                }

                if (thing.FullGround)
                {
                    output.Write(ObdFlags.FullGround);
                }

                if (thing.IgnoreLook)
                {
                    output.Write(ObdFlags.IgnoreLook);
                }

                if (thing.IsCloth)
                {
                    output.Write(ObdFlags.Cloth);
                    output.Write((ushort)thing.ClothSlot);
                }

                if (thing.IsMarketItem)
                {
                    output.Write(ObdFlags.Market);
                    output.Write((ushort)thing.MarketCategory);
                    output.Write(thing.MarketTradeAs);
                    output.Write(thing.MarketShowAs);
                    output.Write((ushort)thing.MarketName.Length);
                    output.Write(thing.MarketName);
                    output.Write(thing.MarketRestrictVocation);
                    output.Write(thing.MarketRestrictLevel);
                }

                if (thing.HasAction)
                {
                    output.Write(ObdFlags.DefaultAction);
                    output.Write((ushort)thing.DefaultAction);
                }

                if (thing.HasCharges)
                {
                    output.Write(ObdFlags.HasChanges);
                }

                if (thing.FloorChange)
                {
                    output.Write(ObdFlags.FloorChange);
                }

                if (thing.Wrappable)
                {
                    output.Write(ObdFlags.Wrappable);
                }

                if (thing.Unwrappable)
                {
                    output.Write(ObdFlags.Unwrappable);
                }

                if (thing.IsTopEffect)
                {
                    output.Write(ObdFlags.TopEffect);
                }

                if (thing.Usable)
                {
                    output.Write(ObdFlags.Usable);
                }
            }

            if (thing.HasLight)
            {
                output.Write(ObdFlags.HasLight);
                output.Write(thing.LightLevel);
                output.Write(thing.LightColor);
            }

            if (thing.HasOffset)
            {
                output.Write(ObdFlags.HasOffset);
                output.Write(thing.OffsetX);
                output.Write(thing.OffsetY);
            }

            if (thing.AnimateAlways)
            {
                output.Write(ObdFlags.AnimateAlways);
            }

            // close flags
            output.Write(ObdFlags.LastFlag);
            return true;
        }

        #endregion
    }
}
