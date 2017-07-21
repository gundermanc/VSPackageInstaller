﻿using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSPackageInstaller.Cache;
using System.Threading;

namespace VSPackageInstaller.UnitTests
{
    [TestClass]
    public class CacheManagerTests
    {
        public string CacheFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_NullCacheFilePath()
        {
            new CacheManager<int>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WhiteSpaceCacheFilePath()
        {
            new CacheManager<int>("  ");
        }

        [TestMethod]
        public void Constructor_InitiallyEmpty()
        {
            var manager = new CacheManager<int>(CacheFilePath);

            Assert.IsFalse(manager.CacheFileExists);
            Assert.IsNull(manager.LastCacheFileUpdateTimeStamp);
            Assert.IsTrue(DateTime.UtcNow.Subtract(manager.LastUpdateTimeStamp.Value).TotalMilliseconds < 100);
            Assert.IsNotNull(manager.Snapshot);
            Assert.AreEqual(0, manager.Snapshot.Count);
        }

        [TestMethod]
        public void CacheFilePath_ResolvesToFullPath()
        {
            var manager = new CacheManager<int>("foofile");

            Assert.IsTrue(manager.CacheFilePath.Split(Path.DirectorySeparatorChar).Length > 1);
        }

        [TestMethod]
        public void CacheFileExists_LastUpdated_CorrectResults()
        {
            var manager = new CacheManager<int>(CacheFilePath);

            Assert.IsFalse(File.Exists(CacheFilePath));
            Assert.IsFalse(manager.CacheFileExists);
            Assert.IsNull(manager.LastCacheFileUpdateTimeStamp);
            Assert.IsTrue(DateTime.UtcNow.Subtract(manager.LastUpdateTimeStamp.Value).TotalMilliseconds < 20);

            Thread.Sleep(1000);

            Assert.IsTrue(DateTime.UtcNow.Subtract(manager.LastUpdateTimeStamp.Value).TotalMilliseconds >= 1000);

            File.WriteAllText(CacheFilePath, string.Empty);

            Assert.IsTrue(File.Exists(CacheFilePath));
            Assert.IsTrue(manager.CacheFileExists);
            Assert.IsTrue(DateTime.UtcNow.Subtract(manager.LastCacheFileUpdateTimeStamp.Value).TotalMilliseconds < 20);
            Assert.IsTrue(DateTime.UtcNow.Subtract(manager.LastUpdateTimeStamp.Value).TotalMilliseconds < 20);

            Thread.Sleep(1000);

            Assert.IsTrue(DateTime.UtcNow.Subtract(manager.LastUpdateTimeStamp.Value).TotalMilliseconds >= 1000);

            File.Delete(CacheFilePath);

            Assert.IsFalse(File.Exists(CacheFilePath));
            Assert.IsFalse(manager.CacheFileExists);
            Assert.IsNull(manager.LastCacheFileUpdateTimeStamp);
            Assert.IsTrue(DateTime.UtcNow.Subtract(manager.LastUpdateTimeStamp.Value).TotalMilliseconds >= 2000);

            manager.AddRange(new[] { 3 });

            Assert.IsTrue(DateTime.UtcNow.Subtract(manager.LastUpdateTimeStamp.Value).TotalMilliseconds < 20);
        }

        [TestMethod]
        public void AddRange_Create_CorrectResults()
        {
            var manager = new CacheManager<int>(CacheFilePath);

            Assert.AreEqual(0, manager.Snapshot.Count);

            manager.AddRange(new[] { 3, 2, 1 });

            Assert.AreEqual(3, manager.Snapshot.Count);
            Assert.AreEqual(3, manager.Snapshot[0]);
            Assert.AreEqual(2, manager.Snapshot[1]);
            Assert.AreEqual(1, manager.Snapshot[2]);

            manager.AddRange(new[] { 7, 8, 9 });

            Assert.AreEqual(6, manager.Snapshot.Count);
            Assert.AreEqual(3, manager.Snapshot[0]);
            Assert.AreEqual(2, manager.Snapshot[1]);
            Assert.AreEqual(1, manager.Snapshot[2]);
            Assert.AreEqual(7, manager.Snapshot[3]);
            Assert.AreEqual(8, manager.Snapshot[4]);
            Assert.AreEqual(9, manager.Snapshot[5]);

            manager.Create(new[] { 4, 5, 6 });

            Assert.AreEqual(3, manager.Snapshot.Count);
            Assert.AreEqual(4, manager.Snapshot[0]);
            Assert.AreEqual(5, manager.Snapshot[1]);
            Assert.AreEqual(6, manager.Snapshot[2]);
        }
    }
}