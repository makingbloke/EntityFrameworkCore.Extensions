// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests
{
    /// <summary>
    /// Tests for ExecuteNonQuery extensions.
    /// </summary>
    [TestClass]
    public class ExecuteNonQueryTests
    {
        /// <summary>
        /// Test ExecuteNonQueryInterpolated.
        /// </summary>
        /// <param name="databaseType">Database type.</param>
        [TestMethod]
        [DataRow(DatabaseType.Sqlite)]
        [DataRow(DatabaseType.SqlServer)]
        public void ExecuteNonQueryInterpolatedTest(DatabaseType databaseType)
        {
            using Context context = DatabaseUtils.CreateDatabase(databaseType);
            long id = DatabaseUtils.CreateSingleTestTableEntry(context, nameof(this.ExecuteNonQueryInterpolatedTest));

            long count = context.Database.ExecuteNonQueryInterpolated($"DELETE FROM TestTable WHERE ID = {id}");
            Assert.AreEqual(1, count);
        }

        /// <summary>
        /// Test ExecuteNonQueryRaw.
        /// </summary>
        /// <param name="databaseType">Database type.</param>
        [TestMethod]
        [DataRow(DatabaseType.Sqlite)]
        [DataRow(DatabaseType.SqlServer)]
        public void ExecuteNonQueryRawTest(DatabaseType databaseType)
        {
            using Context context = DatabaseUtils.CreateDatabase(databaseType);
            long id = DatabaseUtils.CreateSingleTestTableEntry(context, nameof(this.ExecuteNonQueryRawTest));

            long count = context.Database.ExecuteNonQueryRaw("DELETE FROM TestTable WHERE ID = {0}", id);
            Assert.AreEqual(1, count);
        }

        /// <summary>
        /// Test ExecuteNonQueryInterpolatedAsync.
        /// </summary>
        /// <param name="databaseType">Database type.</param>
        /// <returns><see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(DatabaseType.Sqlite)]
        [DataRow(DatabaseType.SqlServer)]
        public async Task ExecuteNonQueryInterpolatedTestAsync(DatabaseType databaseType)
        {
            using Context context = DatabaseUtils.CreateDatabase(databaseType);
            long id = DatabaseUtils.CreateSingleTestTableEntry(context, nameof(this.ExecuteNonQueryInterpolatedTestAsync));

            long count = await context.Database.ExecuteNonQueryInterpolatedAsync($"DELETE FROM TestTable WHERE ID = {id}").ConfigureAwait(false);
            Assert.AreEqual(1, count);
        }

        /// <summary>
        /// Test ExecuteNonQueryRawAsync.
        /// </summary>
        /// <param name="databaseType">Database type.</param>
        /// <returns><see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(DatabaseType.Sqlite)]
        [DataRow(DatabaseType.SqlServer)]
        public async Task ExecuteNonQueryRawTestAsync(DatabaseType databaseType)
        {
            using Context context = DatabaseUtils.CreateDatabase(databaseType);
            long id = DatabaseUtils.CreateSingleTestTableEntry(context, nameof(this.ExecuteNonQueryRawTestAsync));

            long count = await context.Database.ExecuteNonQueryRawAsync("DELETE FROM TestTable WHERE ID = {0}", parameters: id).ConfigureAwait(false);
            Assert.AreEqual(1, count);
        }
    }
}
