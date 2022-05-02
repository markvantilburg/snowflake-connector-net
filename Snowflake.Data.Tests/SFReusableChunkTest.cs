﻿/*
 * Copyright (c) 2012-2019 Snowflake Computing Inc. All rights reserved.
 */

namespace Snowflake.Data.Tests
{
    using NUnit.Framework;
    using System.IO;
    using System.Text;
    using Snowflake.Data.Core;
    using Snowflake.Data.Client;
    using System.Threading.Tasks;

    [TestFixture]
    class SFReusableChunkTest
    {
        [Test]
        public async Task TestSimpleChunk()
        {
            string data = "[ [\"1\", \"1.234\", \"abcde\"],  [\"2\", \"5.678\", \"fghi\"] ]";
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            Stream stream = new MemoryStream(bytes);
            IChunkParser parser = new ReusableChunkParser(stream);

            ExecResponseChunk chunkInfo = new ExecResponseChunk()
            {
                url = "fake",
                uncompressedSize = 100,
                rowCount = 2
            };

            SFReusableChunk chunk = new SFReusableChunk(3);
            chunk.Reset(chunkInfo, 0);

            await parser.ParseChunk(chunk);

            Assert.AreEqual("1", chunk.ExtractCell(0, 0).SafeToString());
            Assert.AreEqual("1.234", chunk.ExtractCell(0, 1).SafeToString());
            Assert.AreEqual("abcde", chunk.ExtractCell(0, 2).SafeToString());
            Assert.AreEqual("2", chunk.ExtractCell(1, 0).SafeToString());
            Assert.AreEqual("5.678", chunk.ExtractCell(1, 1).SafeToString());
            Assert.AreEqual("fghi", chunk.ExtractCell(1, 2).SafeToString());
        }

        [Test]
        public async Task TestChunkWithNull()
        {
            string data = "[ [null, \"1.234\", null],  [\"2\", null, \"fghi\"] ]";
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            Stream stream = new MemoryStream(bytes);
            IChunkParser parser = new ReusableChunkParser(stream);

            ExecResponseChunk chunkInfo = new ExecResponseChunk()
            {
                url = "fake",
                uncompressedSize = 100,
                rowCount = 2
            };

            SFReusableChunk chunk = new SFReusableChunk(3);
            chunk.Reset(chunkInfo, 0);

            await parser.ParseChunk(chunk);

            Assert.AreEqual(null, chunk.ExtractCell(0, 0).SafeToString());
            Assert.AreEqual("1.234", chunk.ExtractCell(0, 1).SafeToString());
            Assert.AreEqual(null, chunk.ExtractCell(0, 2).SafeToString());
            Assert.AreEqual("2", chunk.ExtractCell(1, 0).SafeToString());
            Assert.AreEqual(null, chunk.ExtractCell(1, 1).SafeToString());
            Assert.AreEqual("fghi", chunk.ExtractCell(1, 2).SafeToString());
        }

        [Test]
        public async Task TestChunkWithDate()
        {
            string data = "[ [null, \"2019-08-21T11:58:00\", null],  [\"2\", null, \"fghi\"] ]";
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            Stream stream = new MemoryStream(bytes);
            IChunkParser parser = new ReusableChunkParser(stream);

            ExecResponseChunk chunkInfo = new ExecResponseChunk()
            {
                url = "fake",
                uncompressedSize = 100,
                rowCount = 2
            };

            SFReusableChunk chunk = new SFReusableChunk(3);
            chunk.Reset(chunkInfo, 0);

            await parser.ParseChunk(chunk);

            Assert.AreEqual(null, chunk.ExtractCell(0, 0).SafeToString());
            Assert.AreEqual("2019-08-21T11:58:00", chunk.ExtractCell(0, 1).SafeToString());
            Assert.AreEqual(null, chunk.ExtractCell(0, 2).SafeToString());
            Assert.AreEqual("2", chunk.ExtractCell(1, 0).SafeToString());
            Assert.AreEqual(null, chunk.ExtractCell(1, 1).SafeToString());
            Assert.AreEqual("fghi", chunk.ExtractCell(1, 2).SafeToString());
        }

        [Test]
        public async Task TestChunkWithEscape()
        {
            string data = "[ [\"\\\\åäö\\nÅÄÖ\\r\", \"1.234\", null],  [\"2\", null, \"fghi\"] ]";
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            Stream stream = new MemoryStream(bytes);
            IChunkParser parser = new ReusableChunkParser(stream);

            ExecResponseChunk chunkInfo = new ExecResponseChunk()
            {
                url = "fake",
                uncompressedSize = bytes.Length,
                rowCount = 2
            };

            SFReusableChunk chunk = new SFReusableChunk(3);
            chunk.Reset(chunkInfo, 0);

            await parser.ParseChunk(chunk);

            Assert.AreEqual("\\åäö\nÅÄÖ\r", chunk.ExtractCell(0, 0).SafeToString());
            Assert.AreEqual("1.234", chunk.ExtractCell(0, 1).SafeToString());
            Assert.AreEqual(null, chunk.ExtractCell(0, 2).SafeToString());
            Assert.AreEqual("2", chunk.ExtractCell(1, 0).SafeToString());
            Assert.AreEqual(null, chunk.ExtractCell(1, 1).SafeToString());
            Assert.AreEqual("fghi", chunk.ExtractCell(1, 2).SafeToString());
        }

        [Test]
        public async Task TestChunkWithLongString()
        {
            string longstring = new string('å', 10 * 1000 * 1000);
            string data = "[ [\"åäö\\nÅÄÖ\\r\", \"1.234\", null],  [\"2\", null, \"" + longstring + "\"] ]";
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            Stream stream = new MemoryStream(bytes);
            IChunkParser parser = new ReusableChunkParser(stream);

            ExecResponseChunk chunkInfo = new ExecResponseChunk()
            {
                url = "fake",
                uncompressedSize = bytes.Length,
                rowCount = 2
            };

            SFReusableChunk chunk = new SFReusableChunk(3);
            chunk.Reset(chunkInfo, 0);

            await parser.ParseChunk(chunk);

            Assert.AreEqual("åäö\nÅÄÖ\r", chunk.ExtractCell(0, 0).SafeToString());
            Assert.AreEqual("1.234", chunk.ExtractCell(0, 1).SafeToString());
            Assert.AreEqual(null, chunk.ExtractCell(0, 2).SafeToString());
            Assert.AreEqual("2", chunk.ExtractCell(1, 0).SafeToString());
            Assert.AreEqual(null, chunk.ExtractCell(1, 1).SafeToString());
            Assert.AreEqual(longstring, chunk.ExtractCell(1, 2).SafeToString());
        }

        [Test]
        public async Task TestParserError1()
        {
            // Unterminated escape sequence
            string data = "[ [\"åäö\\";
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            Stream stream = new MemoryStream(bytes);
            IChunkParser parser = new ReusableChunkParser(stream);

            ExecResponseChunk chunkInfo = new ExecResponseChunk()
            {
                url = "fake",
                uncompressedSize = bytes.Length,
                rowCount = 1
            };

            SFReusableChunk chunk = new SFReusableChunk(1);
            chunk.Reset(chunkInfo, 0);

            try
            {
                await parser.ParseChunk(chunk);
                Assert.Fail();
            }
            catch (SnowflakeDbException e)
            {
                Assert.AreEqual(SFError.INTERNAL_ERROR.GetAttribute<SFErrorAttr>().errorCode, e.ErrorCode);
            }
        }

        [Test]
        public async Task TestParserError2()
        {
            // Unterminated string
            string data = "[ [\"åäö";
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            Stream stream = new MemoryStream(bytes);
            IChunkParser parser = new ReusableChunkParser(stream);

            ExecResponseChunk chunkInfo = new ExecResponseChunk()
            {
                url = "fake",
                uncompressedSize = bytes.Length,
                rowCount = 1
            };

            SFReusableChunk chunk = new SFReusableChunk(1);
            chunk.Reset(chunkInfo, 0);

            try
            {
                await parser.ParseChunk(chunk);
                Assert.Fail();
            }
            catch (SnowflakeDbException e)
            {
                Assert.AreEqual(SFError.INTERNAL_ERROR.GetAttribute<SFErrorAttr>().errorCode, e.ErrorCode);
            }
        }

        [Test]
        public async Task TestParserWithTab()
        {
            // Unterminated string
            string data = "[[\"abc\t\"]]";
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            Stream stream = new MemoryStream(bytes);
            IChunkParser parser = new ReusableChunkParser(stream);

            ExecResponseChunk chunkInfo = new ExecResponseChunk()
            {
                url = "fake",
                uncompressedSize = bytes.Length,
                rowCount = 1
            };

            SFReusableChunk chunk = new SFReusableChunk(1);
            chunk.Reset(chunkInfo, 0);

            await parser.ParseChunk(chunk);
            string val = chunk.ExtractCell(0, 0).SafeToString();
            Assert.AreEqual("abc\t", chunk.ExtractCell(0, 0).SafeToString());
        }
    }
}