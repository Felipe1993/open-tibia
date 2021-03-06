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
using OpenTibia.Core;
#endregion

namespace OpenTibia.Client.Sprites
{
    public delegate void SpriteListChangedHandler(object sender, SpriteListChangedArgs e);

    public class SpriteListChangedArgs
    {
        #region Constructor

        public SpriteListChangedArgs(Sprite[] changedSprites, StorageChangeType changeType)
        {
            this.ChangedSprites = changedSprites;
            this.ChangeType = changeType;
        }

        #endregion

        #region Public Properties

        public Sprite[] ChangedSprites { get; private set; }

        public StorageChangeType ChangeType { get; private set; }

        #endregion
    }
}
