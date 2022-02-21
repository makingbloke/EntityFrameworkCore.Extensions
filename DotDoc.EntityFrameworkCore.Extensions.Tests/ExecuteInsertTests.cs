// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests
{
    /// <summary>
    /// Tests for ExecuteInsert extensions.
    /// </summary>
    [TestClass]
    public class ExecuteInsertTests
    {
        /// <summary>
        /// Test ExecuteInsertInterpolated.
        /// </summary>
        /// <param name="databaseType">Database type.</param>
        [TestMethod]
        [DataRow(DatabaseType.Sqlite)]
        [DataRow(DatabaseType.SqlServer)]
        public void ExecuteInsertInterpolatedTest(DatabaseType databaseType)
        {
            const string value = nameof(this.ExecuteInsertInterpolatedTest);

            using Context context = DatabaseUtils.CreateDatabase(databaseType);

            long id = context.Database.ExecuteInsertInterpolated($"INSERT INTO TestTable(TestField) VALUES ({value})");
            Assert.AreEqual(1, id);

            TestTable testTable = context.TestTable.FirstOrDefault(e => e.Id == id);
            Assert.AreEqual(value, testTable?.TestField);
        }

        /// <summary>
        /// Test ExecuteInsertRaw.
        /// </summary>
        /// <param name="databaseType">Database type.</param>
        [TestMethod]
        [DataRow(DatabaseType.Sqlite)]
        [DataRow(DatabaseType.SqlServer)]
        public void ExecuteInsertRawTest(DatabaseType databaseType)
        {
            const string value = nameof(this.ExecuteInsertInterpolatedTest);

            using Context context = DatabaseUtils.CreateDatabase(databaseType);

            long id = context.Database.ExecuteInsertRaw("INSERT INTO TestTable(TestField) VALUES ({0})", value);
            Assert.AreEqual(1, id);

            TestTable testTable = context.TestTable.FirstOrDefault(e => e.Id == id);
            Assert.AreEqual(value, testTable?.TestField);
        }

        /// <summary>
        /// Test ExecuteInsertInterpolatedAsync.
        /// </summary>
        /// <param name="databaseType">Database type.</param>
        /// <returns><see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(DatabaseType.Sqlite)]
        [DataRow(DatabaseType.SqlServer)]
        public async Task ExecuteInsertInterpolatedTestAsync(DatabaseType databaseType)
        {
            const string value = nameof(this.ExecuteInsertInterpolatedTest);

            using Context context = DatabaseUtils.CreateDatabase(databaseType);

            long id = await context.Database.ExecuteInsertInterpolatedAsync($"INSERT INTO TestTable(TestField) VALUES ({value})").ConfigureAwait(false);
            Assert.AreEqual(1, id);

            TestTable testTable = context.TestTable.FirstOrDefault(e => e.Id == id);
            Assert.AreEqual(value, testTable?.TestField);
        }

        /// <summary>
        /// Test ExecuteInsertRawAsync.
        /// </summary>
        /// <param name="databaseType">Database type.</param>
        /// <returns><see cref="Task"/>.</returns>
        [TestMethod]
        [DataRow(DatabaseType.Sqlite)]
        [DataRow(DatabaseType.SqlServer)]
        public async Task ExecuteInsertRawTestAsync(DatabaseType databaseType)
        {
            const string value = nameof(this.ExecuteInsertInterpolatedTest);

            using Context context = DatabaseUtils.CreateDatabase(databaseType);

            long id = await context.Database.ExecuteInsertRawAsync("INSERT INTO TestTable(TestField) VALUES ({0})", parameters: value).ConfigureAwait(false);
            Assert.AreEqual(1, id);

            TestTable testTable = context.TestTable.FirstOrDefault(e => e.Id == id);
            Assert.AreEqual(value, testTable?.TestField);
        }
    }
}
