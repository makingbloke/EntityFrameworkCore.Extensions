// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.CaseInsensitivity;

/// <summary>
/// Tests for the UseCaseInsensitiveCollation extension.
/// </summary>
[TestClass]
public class UseCaseInsensitiveCollationTests
{
    #region private constants

    /// <summary>
    /// The <see cref="ConvertedText.Code"/> value converter is set with lambdas: <c>HasConversion(code => ..., value => ...)</c>.
    /// </summary>
    private const string LambdaConversion = "Lambda";

    /// <summary>
    /// The <see cref="ConvertedText.Code"/> value converter is set with an instance: <c>HasConversion(new TextCodeConverter())</c>.
    /// </summary>
    private const string ConverterInstanceConversion = "ConverterInstance";

    /// <summary>
    /// The <see cref="ConvertedText.Code"/> value converter is set with its type: <c>HasConversion&lt;TextCodeConverter&gt;()</c>.
    /// </summary>
    private const string ConverterTypeConversion = "ConverterType";

    /// <summary>
    /// The <see cref="ConvertedText.Code"/> value converter is set for every <see cref="TextCode"/> in ConfigureConventions.
    /// </summary>
    private const string ConventionConversion = "Convention";

    #endregion private constants

    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="UseCaseInsensitiveCollationTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public UseCaseInsensitiveCollationTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region UseCaseInsensitiveCollation tests

    /// <summary>
    /// Test UseCaseInsensitiveCollation with a Null ModelBuilder parameter.
    /// </summary>
    [TestMethod]
    public void UseCaseInsensitiveCollation_NullModelBuilder_ThrowsArgumentNullException()
    {
        // ARRANGE
        ModelBuilder modelBuilder = null!;
        string databaseType = DatabaseTypes.Sqlite;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => modelBuilder.UseCaseInsensitiveCollation(databaseType), "Unexpected exception");
    }

    /// <summary>
    /// Test UseCaseInsensitiveCollation with an unsupported database type.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(null, DisplayName = "UnsupportedDatabaseType Null")]
    [DataRow("", DisplayName = "UnsupportedDatabaseType Empty")]
    [DataRow("Invalid", DisplayName = "UnsupportedDatabaseType Invalid")]
    public async Task UseCaseInsensitiveCollation_UnsupportedDatabaseType_ThrowsUnsupportedDatabaseTypeException(string databaseType)
    {
        // ARRANGE

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<UnsupportedDatabaseTypeException>(
            async () =>
            {
                using Context context = await DatabaseUtils.OpenDatabaseAsync(
                    this._testContext,
                    DatabaseTypes.Sqlite,
                    customModelCreationActions: modelBuilder => modelBuilder.UseCaseInsensitiveCollation(databaseType))
                    .ConfigureAwait(false);
            },
            "Unexpected exception")
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Test UseCaseInsensitiveCollation.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"DifferentCase {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"DifferentCase {DatabaseTypes.SqlServer}")]
    public async Task UseCaseInsensitiveCollation_DifferentCase_Matches(string databaseType)
    {
        // ARRANGE / ACT
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customModelCreationActions: modelBuilder => modelBuilder.UseCaseInsensitiveCollation(databaseType))
            .ConfigureAwait(false);

        string originalValue = new('a', 10);
        string searchValue = originalValue.ToUpperInvariant();

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 1, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        List<TestTable1> results = await context.TestTable1
            .Where(e => e.TestField == searchValue)
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(1, results, "Invalid count");
    }

    /// <summary>
    /// Test UseCaseInsensitiveCollation is case insensitive but accent sensitive on both databases.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="originalValue">The value stored in the database.</param>
    /// <param name="searchValue">The value to search for.</param>
    /// <param name="expectedCount">The expected number of matching records.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// SQLite uses the NOCASE collation, replaced by UseSqliteNoCaseReplacement so that it handles non-ASCII characters.
    /// SQL Server uses Latin1_General_100_CI_AS_SC_UTF8. The same values must give the same results on both.
    /// </remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, "ááá", "ÁÁÁ", 1, DisplayName = $"CaseAndAccents {DatabaseTypes.Sqlite} Accented lower case matches accented upper case")]
    [DataRow(DatabaseTypes.Sqlite, "ááá", "AAA", 0, DisplayName = $"CaseAndAccents {DatabaseTypes.Sqlite} Accented does not match unaccented")]
    [DataRow(DatabaseTypes.Sqlite, "aaa", "ÁÁÁ", 0, DisplayName = $"CaseAndAccents {DatabaseTypes.Sqlite} Unaccented does not match accented")]
    [DataRow(DatabaseTypes.Sqlite, "abc", "ABC", 1, DisplayName = $"CaseAndAccents {DatabaseTypes.Sqlite} ASCII lower case matches upper case")]
    [DataRow(DatabaseTypes.Sqlite, "æøå", "ÆØÅ", 1, DisplayName = $"CaseAndAccents {DatabaseTypes.Sqlite} Non-ASCII lower case matches upper case")]
    [DataRow(DatabaseTypes.Sqlite, "ＡＢＣ", "abc", 1, DisplayName = $"CaseAndAccents {DatabaseTypes.Sqlite} Full width matches half width")]
    [DataRow(DatabaseTypes.SqlServer, "ááá", "ÁÁÁ", 1, DisplayName = $"CaseAndAccents {DatabaseTypes.SqlServer} Accented lower case matches accented upper case")]
    [DataRow(DatabaseTypes.SqlServer, "ááá", "AAA", 0, DisplayName = $"CaseAndAccents {DatabaseTypes.SqlServer} Accented does not match unaccented")]
    [DataRow(DatabaseTypes.SqlServer, "aaa", "ÁÁÁ", 0, DisplayName = $"CaseAndAccents {DatabaseTypes.SqlServer} Unaccented does not match accented")]
    [DataRow(DatabaseTypes.SqlServer, "abc", "ABC", 1, DisplayName = $"CaseAndAccents {DatabaseTypes.SqlServer} ASCII lower case matches upper case")]
    [DataRow(DatabaseTypes.SqlServer, "æøå", "ÆØÅ", 1, DisplayName = $"CaseAndAccents {DatabaseTypes.SqlServer} Non-ASCII lower case matches upper case")]
    [DataRow(DatabaseTypes.SqlServer, "ＡＢＣ", "abc", 1, DisplayName = $"CaseAndAccents {DatabaseTypes.SqlServer} Full width matches half width")]
    public async Task UseCaseInsensitiveCollation_CaseAndAccents_IgnoresCaseNotAccents(string databaseType, string originalValue, string searchValue, int expectedCount)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder =>
            {
                if (databaseType == DatabaseTypes.Sqlite)
                {
                    optionsBuilder.UseSqliteNoCaseReplacement();
                }
            },
            customModelCreationActions: modelBuilder => modelBuilder.UseCaseInsensitiveCollation(databaseType))
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 1, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        List<TestTable1> results = await context.TestTable1
            .Where(e => e.TestField == searchValue)
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(expectedCount, results, "Invalid record count");
    }

    /// <summary>
    /// Test UseCaseInsensitiveCollation on SQLite keeps the collation of a property that already has one, and sets NOCASE on the others.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>TestTable1.TestField is given the BINARY collation before UseCaseInsensitiveCollation is called, so it stays case sensitive.</remarks>
    [TestMethod]
    public async Task UseCaseInsensitiveCollation_SqliteExistingCollation_KeepsIt()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.Sqlite,
            customModelCreationActions: modelBuilder =>
            {
                modelBuilder.Entity<TestTable1>().Property(e => e.TestField).UseCollation("BINARY");
                modelBuilder.UseCaseInsensitiveCollation(DatabaseTypes.Sqlite);
            })
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "abc", 1, this._testContext.CancellationToken).ConfigureAwait(false);
        context.TestTable2.Add(new TestTable2 { TestField = "abc" });
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        int binaryCount = await context.TestTable1
            .CountAsync(e => e.TestField == "ABC", this._testContext.CancellationToken)
            .ConfigureAwait(false);

        int noCaseCount = await context.TestTable2
            .CountAsync(e => e.TestField == "ABC", this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(0, binaryCount, "The existing BINARY collation was replaced");
        Assert.AreEqual(1, noCaseCount, "NOCASE was not set on a property without a collation");
    }

    /// <summary>
    /// Test UseCaseInsensitiveCollation makes the string properties of a complex type, and of a nested complex type, case insensitive.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"ComplexType {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"ComplexType {DatabaseTypes.SqlServer}")]
    public async Task UseCaseInsensitiveCollation_ComplexType_IgnoresCase(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customModelCreationActions: modelBuilder =>
            {
                // Before UseCaseInsensitiveCollation, which only sets the collation on the entity types already in the model.
                modelBuilder.Entity<CollationOwner>().ComplexProperty(e => e.Part, part => part.ComplexProperty(p => p.SubPart));
                modelBuilder.UseCaseInsensitiveCollation(databaseType);
            })
            .ConfigureAwait(false);

        context.Add(new CollationOwner { Part = new CollationPart { Text = "abc", SubPart = new CollationSubPart { Text = "def" } } });
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        int partCount = await context.Set<CollationOwner>()
            .CountAsync(e => e.Part.Text == "ABC", this._testContext.CancellationToken)
            .ConfigureAwait(false);

        int subPartCount = await context.Set<CollationOwner>()
            .CountAsync(e => e.Part.SubPart.Text == "DEF", this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(1, partCount, "The complex type's property is case sensitive");
        Assert.AreEqual(1, subPartCount, "The nested complex type's property is case sensitive");
    }

    /// <summary>
    /// Test UseCaseInsensitiveCollation on SQLite keeps the collation of a complex type's property that already has one.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>CollationPart.Text is given the BINARY collation before UseCaseInsensitiveCollation is called, so it stays case sensitive.</remarks>
    [TestMethod]
    public async Task UseCaseInsensitiveCollation_SqliteComplexTypeExistingCollation_KeepsIt()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.Sqlite,
            customModelCreationActions: modelBuilder =>
            {
                modelBuilder.Entity<CollationOwner>().ComplexProperty(
                    e => e.Part,
                    part =>
                    {
                        part.Property(p => p.Text).UseCollation("BINARY");
                        part.ComplexProperty(p => p.SubPart);
                    });

                modelBuilder.UseCaseInsensitiveCollation(DatabaseTypes.Sqlite);
            })
            .ConfigureAwait(false);

        context.Add(new CollationOwner { Part = new CollationPart { Text = "abc", SubPart = new CollationSubPart { Text = "def" } } });
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        int binaryCount = await context.Set<CollationOwner>()
            .CountAsync(e => e.Part.Text == "ABC", this._testContext.CancellationToken)
            .ConfigureAwait(false);

        int noCaseCount = await context.Set<CollationOwner>()
            .CountAsync(e => e.Part.SubPart.Text == "DEF", this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(0, binaryCount, "The existing BINARY collation was replaced");
        Assert.AreEqual(1, noCaseCount, "NOCASE was not set on a property without a collation");
    }

    /// <summary>
    /// Test UseCaseInsensitiveCollation on SQLite leaves the properties of a complex type mapped to JSON without a collation, as they're stored
    /// inside the JSON column, not in columns of their own.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task UseCaseInsensitiveCollation_SqliteJsonComplexType_SetsNoCollation()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.Sqlite,
            customModelCreationActions: modelBuilder =>
            {
                modelBuilder.Entity<CollationOwner>().ComplexProperty(
                    e => e.Part,
                    part =>
                    {
                        part.ToJson();
                        part.ComplexProperty(p => p.SubPart);
                    });

                modelBuilder.UseCaseInsensitiveCollation(DatabaseTypes.Sqlite);
            })
            .ConfigureAwait(false);

        context.Add(new CollationOwner { Part = new CollationPart { Text = "abc", SubPart = new CollationSubPart { Text = "def" } } });
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        IComplexType partType = context.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(CollationOwner))!
            .FindComplexProperty(nameof(CollationOwner.Part))!
            .ComplexType;

        // ACT
        int count = await context.Set<CollationOwner>()
            .CountAsync(e => e.Part.Text == "abc" && e.Part.SubPart.Text == "def", this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.IsNull(partType.FindProperty(nameof(CollationPart.Text))!.GetCollation(), "A collation was set inside the JSON column");
        Assert.IsNull(partType.FindComplexProperty(nameof(CollationPart.SubPart))!.ComplexType.FindProperty(nameof(CollationSubPart.Text))!.GetCollation(), "A collation was set inside the JSON column");
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test UseCaseInsensitiveCollation makes a property stored as a string by a value converter case insensitive, however the converter is set.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="conversion">How the value converter is set (one of the conversion constants).</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, LambdaConversion, DisplayName = $"ValueConverter {DatabaseTypes.Sqlite} {LambdaConversion}")]
    [DataRow(DatabaseTypes.Sqlite, ConverterInstanceConversion, DisplayName = $"ValueConverter {DatabaseTypes.Sqlite} {ConverterInstanceConversion}")]
    [DataRow(DatabaseTypes.Sqlite, ConverterTypeConversion, DisplayName = $"ValueConverter {DatabaseTypes.Sqlite} {ConverterTypeConversion}")]
    [DataRow(DatabaseTypes.Sqlite, ConventionConversion, DisplayName = $"ValueConverter {DatabaseTypes.Sqlite} {ConventionConversion}")]
    [DataRow(DatabaseTypes.SqlServer, LambdaConversion, DisplayName = $"ValueConverter {DatabaseTypes.SqlServer} {LambdaConversion}")]
    [DataRow(DatabaseTypes.SqlServer, ConverterInstanceConversion, DisplayName = $"ValueConverter {DatabaseTypes.SqlServer} {ConverterInstanceConversion}")]
    [DataRow(DatabaseTypes.SqlServer, ConverterTypeConversion, DisplayName = $"ValueConverter {DatabaseTypes.SqlServer} {ConverterTypeConversion}")]
    [DataRow(DatabaseTypes.SqlServer, ConventionConversion, DisplayName = $"ValueConverter {DatabaseTypes.SqlServer} {ConventionConversion}")]
    public async Task UseCaseInsensitiveCollation_ValueConverter_IgnoresCase(string databaseType, string conversion)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customModelCreationActions: modelBuilder =>
            {
                // Before UseCaseInsensitiveCollation, which only sets the collation on the entity types already in the model.
                AddConvertedText(modelBuilder, conversion);
                modelBuilder.UseCaseInsensitiveCollation(databaseType);
            },
            customConventionActions: configurationBuilder =>
            {
                if (conversion == ConventionConversion)
                {
                    configurationBuilder.Properties<TextCode>().HaveConversion<TextCodeConverter>();
                }
            })
            .ConfigureAwait(false);

        context.Add(new ConvertedText { Code = new TextCode("ABC") });
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        int count = await context.Set<ConvertedText>()
            .CountAsync(e => e.Code == new TextCode("abc"), this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(1, count, "The property stored as a string by a value converter is case sensitive");
    }

    /// <summary>
    /// Test UseCaseInsensitiveCollation makes a char property, and a nullable char property, case insensitive.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"Char {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"Char {DatabaseTypes.SqlServer}")]
    public async Task UseCaseInsensitiveCollation_Char_IgnoresCase(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customModelCreationActions: modelBuilder =>
            {
                // Before UseCaseInsensitiveCollation, which only sets the collation on the entity types already in the model.
                AddConvertedText(modelBuilder, LambdaConversion);
                modelBuilder.UseCaseInsensitiveCollation(databaseType);
            })
            .ConfigureAwait(false);

        context.Add(new ConvertedText { Grade = 'A', NullableGrade = 'A' });
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        int gradeCount = await context.Set<ConvertedText>()
            .CountAsync(e => e.Grade == 'a', this._testContext.CancellationToken)
            .ConfigureAwait(false);

        int nullableGradeCount = await context.Set<ConvertedText>()
            .CountAsync(e => e.NullableGrade == 'a', this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(1, gradeCount, "The char property is case sensitive");
        Assert.AreEqual(1, nullableGradeCount, "The nullable char property is case sensitive");
    }

    /// <summary>
    /// Test UseCaseInsensitiveCollation on SQLite sets NOCASE on the properties whose CLR type isn't string but which are stored as text,
    /// and not on those stored as numbers.
    /// </summary>
    /// <param name="propertyName">The name of the <see cref="ConvertedText"/> property.</param>
    /// <param name="expectedCollation">The collation expected on the property, or <see langword="null"/> for none.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(nameof(ConvertedText.Code), SqliteUnicodeFunctionNames.NoCaseCollation, DisplayName = "SqliteNonStringProperty Value converter to string")]
    [DataRow(nameof(ConvertedText.Grade), SqliteUnicodeFunctionNames.NoCaseCollation, DisplayName = "SqliteNonStringProperty Char")]
    [DataRow(nameof(ConvertedText.NullableGrade), SqliteUnicodeFunctionNames.NoCaseCollation, DisplayName = "SqliteNonStringProperty Nullable char")]
    [DataRow(nameof(ConvertedText.Day), SqliteUnicodeFunctionNames.NoCaseCollation, DisplayName = "SqliteNonStringProperty Enum stored as string")]
    [DataRow(nameof(ConvertedText.GradeNumber), null, DisplayName = "SqliteNonStringProperty Char stored as number")]
    [DataRow(nameof(ConvertedText.DayNumber), null, DisplayName = "SqliteNonStringProperty Enum stored as number")]
    [DataRow(nameof(ConvertedText.Id), null, DisplayName = "SqliteNonStringProperty Number")]
    public async Task UseCaseInsensitiveCollation_SqliteNonStringProperty_SetsNoCaseOnlyOnText(string propertyName, string? expectedCollation)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.Sqlite,
            customModelCreationActions: modelBuilder =>
            {
                // Before UseCaseInsensitiveCollation, which only sets the collation on the entity types already in the model.
                AddConvertedText(modelBuilder, LambdaConversion);
                modelBuilder.UseCaseInsensitiveCollation(DatabaseTypes.Sqlite);
            })
            .ConfigureAwait(false);

        // ACT
        string? collation = context.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(ConvertedText))!
            .FindProperty(propertyName)!
            .GetCollation();

        // ASSERT
        Assert.AreEqual(expectedCollation, collation, "Invalid collation");
    }

    #endregion UseCaseInsensitiveCollation tests

    #region private methods

    /// <summary>
    /// Adds <see cref="ConvertedText"/> to the model, with its value converters.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    /// <param name="conversion">
    /// How the <see cref="ConvertedText.Code"/> value converter is set (one of the conversion constants). With <see cref="ConventionConversion"/>,
    /// the test sets it in its custom convention actions.
    /// </param>
    private static void AddConvertedText(ModelBuilder modelBuilder, string conversion)
    {
        EntityTypeBuilder<ConvertedText> entityTypeBuilder = modelBuilder.Entity<ConvertedText>();
        PropertyBuilder<TextCode> codeBuilder = entityTypeBuilder.Property(e => e.Code);

        switch (conversion)
        {
            case LambdaConversion:
                codeBuilder.HasConversion(code => code.Value, value => new TextCode(value));
                break;

            case ConverterInstanceConversion:
                codeBuilder.HasConversion(new TextCodeConverter());
                break;

            case ConverterTypeConversion:
                codeBuilder.HasConversion<TextCodeConverter>();
                break;

            case ConventionConversion:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(conversion), conversion, "Unknown conversion");
        }

        entityTypeBuilder.Property(e => e.GradeNumber).HasConversion<int>();
        entityTypeBuilder.Property(e => e.Day).HasConversion<string>();
    }

    #endregion private methods
}