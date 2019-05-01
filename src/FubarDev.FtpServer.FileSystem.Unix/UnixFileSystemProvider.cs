// <copyright file="UnixFileSystemProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

using Mono.Unix;

namespace FubarDev.FtpServer.FileSystem.Unix
{
    /// <summary>
    /// A file system provider that uses the Posix API.
    /// </summary>
    public class UnixFileSystemProvider : IFileSystemClassFactory
    {
        /// <inheritdoc />
        public Task<IUnixFileSystem> Create(IAccountInformation accountInformation)
        {
            var userInfo = new UnixUserInfo(accountInformation.User.Name);
            if (userInfo == null)
            {
                throw new InvalidOperationException($"The user could not be found by getent().");
            }

            var root = new UnixDirectoryInfo(userInfo.HomeDirectory);
            var rootEntry = new UnixDirectoryEntry(root, accountInformation.User, userInfo);
            return Task.FromResult<IUnixFileSystem>(new UnixFileSystem(rootEntry, accountInformation.User, userInfo));
        }
    }
}