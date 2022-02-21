// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests
{
    /// <summary>
    /// Tests for ExecuteScalar extensions.
    /// </summary>
    [TestClass]
    public class ExecuteScalarTests
    {
        /// <summary>
        /// Test ExecuteScalarInterpolated.
        /// </summary>
        /// <param name="databaseType">Database type.</param>
        [TestMethod]
        [DataRow(DatabaseType.Sqlite)]
        [DataRow(DatabaseType.SqlServer)]
        public void ExecuteScalarInterpolatedTest(DatabaseType databaseType)
        {
            const string value = nameof(this.ExecuteScalarInterpolatedTest);

            using Context context = DatabaseUtils.CreateDatabase(databaseType);
            long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);

            string actualValue = context.Database.ExecuteScalarInterpolated<string>($"SELECT TestField FROM TestTable WHERE ID = {id}");
            Assert.AreEqual(value, actualValue);
        }

        /// <summary>
        /// Test ExecuteScalarRaw.
        /// </summary>
        /// <param name="databaseType">Database type.</param>
        [TestMethod]
        [DataRow(DatabaseType.Sqlite)]
        [DataRow(DatabaseType.SqlServer)]
        public void ExecuteScalarRawTest(DatabaseType databaseType)
        {
            const string value = nameof(this.ExecuteScalarRawTest);

            using Context context = DatabaseUtils.CreateDatabase(databaseType);
            long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);

            string actualValue = context.Database.ExecuteScalarRaw<string>("SELECT TestField FROM TestTable WHERE ID = {0}", id);
            Assert.AreEqual(value, actualValue);
        }

        /// <summary>
        /// Test ExecuteScalarInterpolatedAsync.
        /// </summary>
        /// <param name="databaseType">Database type.</param>
        /// <returns><see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(DatabaseType.Sqlite)]
        [DataRow(DatabaseType.SqlServer)]
        public async Task ExecuteScalarInterpolatedTestAsync(DatabaseType databaseType)
        {
            const string value = nameof(this.ExecuteScalarInterpolatedTestAsync);

            using Context context = DatabaseUtils.CreateDatabase(databaseType);
            long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);

            string actualValue = await context.Database.ExecuteScalarInterpolatedAsync<string>($"SELECT TestField FROM TestTable WHERE ID = {id}").ConfigureAwait(false);
            Assert.AreEqual(value, actualValue);
        }

        /// <summary>
        /// Test ExecuteScalarRawAsync.
        /// </summary>
        /// <param name="databaseType">Database type.</param>
        /// <returns><see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(DatabaseType.Sqlite)]
        [DataRow(DatabaseType.SqlServer)]
        public async Task ExecuteScalarRawTestAsync(DatabaseType databaseType)
        {
            const string value = nameof(this.ExecuteScalarRawTestAsync);

            using Context context = DatabaseUtils.CreateDatabase(databaseType);
            long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);

            string actualValue = await context.Database.ExecuteScalarRawAsync<string>("SELECT TestField FROM TestTable WHERE ID = {0}", parameters: id).ConfigureAwait(false);
            Assert.AreEqual(value, actualValue);
        }
    }
}
