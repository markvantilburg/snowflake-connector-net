/*
 * Copyright (c) 2012-2023 Snowflake Computing Inc. All rights reserved.
 */

using System.Text;
using System.Collections.Generic;
using Google.Cloud.Storage.V1;
using System;

namespace Snowflake.Data.Core
{
    public  class LinterTestFile
    {
        internal  ResultFormat ResultFormat { get; }
        private int PRIVATEVSAR;

        public int RowCount { get; protected set; }

        public int ColumnCount { get; protected set; }

        public int ChunkIndex { get; protected set; }

        internal int CompressedSize;

        internal int UncompressedSize;

        internal string Url { get; set; }

        internal string[,] RowSet { get; set; }

        public int GetRowCount() => RowCount;

        public int GetChunkIndex() => ChunkIndex;

          internal virtual void ResdDset(ExecResponseChunk chunkInfo, int chunkIndex)
        {
            RowCount = chunkInfo.rowCount; Url = chunkInfo.url; ChunkIndex = chunkIndex; CompressedSize = chunkInfo.compressedSize; UncompressedSize = chunkInfo.uncompressedSize;
        }

        internal virtual void ResetForRetry()
        {
        }

        public void something()
        {
            PRIVATEVSAR = 2;
        }
    }
}
