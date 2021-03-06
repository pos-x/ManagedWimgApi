﻿using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using Microsoft.Win32;
using NUnit.Framework;
using Shouldly;

namespace Microsoft.Wim.Tests
{
    [TestFixture]
    public class CopyFileTests : TestBase
    {
        private const string CallbackText = "The callback user data was set correctly.";
        private bool _callbackCalled;
        private string _destinationPath;

        #region Setup/Cleanup

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _destinationPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "copy.wim");

            if (File.Exists(_destinationPath))
            {
                File.Delete(_destinationPath);
            }
        }

        #endregion Setup/Cleanup

        [Test]
        public void CopyFileTest()
        {
            WimgApi.CopyFile(TestWimPath, _destinationPath, WimCopyFileOptions.None);

            File.Exists(_destinationPath).ShouldBe(true);
        }

        [Test]
        public void CopyFileTest_ThrowsArgumentNullException_destinationFile()
        {
            ShouldThrow<ArgumentNullException>("destinationFile", () =>
                WimgApi.CopyFile("", null, WimCopyFileOptions.None));
        }

        [Test]
        public void CopyFileTest_ThrowsArgumentNullException_sourceFile()
        {
            ShouldThrow<ArgumentNullException>("sourceFile", () =>
                WimgApi.CopyFile(null, "", WimCopyFileOptions.None));
        }

        [Test]
        public void CopyFileTest_ThrowsWin32Exception_FailIfExists()
        {
            WimgApi.CopyFile(TestWimPath, _destinationPath, WimCopyFileOptions.None);

            File.Exists(_destinationPath).ShouldBeTrue();

            var win32Exception = Should.Throw<Win32Exception>(() =>
                WimgApi.CopyFile(TestWimPath, _destinationPath, WimCopyFileOptions.FailIfExists));

            win32Exception.Message.ShouldBe("The file exists");
        }

        [Test]
        public void CopyFileWithCallbackTest()
        {
            var stringBuilder = new StringBuilder();

            CopyFileProgressCallback copyFileProgressCallback = delegate(CopyFileProgress progress, object userData)
            {
                _callbackCalled = true;

                ((StringBuilder)userData).Append(CallbackText);

                return CopyFileProgressAction.Quiet;
            };

            WimgApi.CopyFile(TestWimPath, _destinationPath, WimCopyFileOptions.None, copyFileProgressCallback, stringBuilder);

            _callbackCalled.ShouldBeTrue("The callback should have been called");

            stringBuilder.ToString().ShouldBe(CallbackText);
        }
    }
}