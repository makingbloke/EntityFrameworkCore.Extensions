
### Version 5.0.0.2 - 11th June 2025

* Upgraded SonarAnalyzer, EF Core and MSTest Nuget packages.

### Version 5.0.0.1 - 18th May 2025

* Upgraded solution to use new XML based .slnx format.

### Version 5.0.0.0 - 16th May 2025

* **Breaking Change** Refactored code so it is grouped into namespaces based on functionality.
* **Breaking Change** Removed Synchronous methods where an asynchronous alternative exists.
* **Breaking Change** The project now needs the full version of EF core.
* **Breaking Change** Removed ``string GetDatabaseType(string providerName)`` as this is no longer needed, use one of the other overloads. 
* `ExecuteUpdateGetRows` now alters how it obtains its output depending on the value in the `UseSqlReturningClause` / `UseSqlOutputClause` model builder options.
* Re-instated the `GetDbContext` extensions as the IQueryable variant is needed by `ExecuteUpdateGetRows`.
* When testing with SQLite changed the tests to use a physical rather than an in memory database. 
* Upgraded SonarAnalyzer, EF Core and MSTest Nuget packages.
* Refactored update setters and get DbContext to make Reflection code more efficient.
* Added new `Match` and `FreeTextSearch` DbFunction extensions to allow for free text searching in LINQ queries. 

### Version 4.0.1.1 - 03rd March 2025

* Upgraded SonarAnalyzer, EF Core and MSTest Nuget packages.

### Version 4.0.1.0 - 10 February 2025

* Added new Case Insensitivity Extensions which provide an upgrade to SQLite's NoCase collation and methods to set a case insensitive default collation sequence.
* Updated the MSTest Nuget package.
* More Test refactoring and tidying.

### Version 4.0.0.1 - 04 February 2025

* Fixed issue where `ExecuteUpdateGetRows` was not finding any rows to update due to an issue with parameter types when run against SQLite.

### Version 4.0.0.0 - 31st January 2025

* Updated copyright date to 2025.
* Upgraded SonarAnalyzer, EF Core and MSTest Nuget packages.
* **Breaking Change** Moved extensions into Extension namespace and general code into Classes.
* **Breaking Change** Renamed UniqueConstraintInterceptor to UniqueConstraintSaveChangesInterceptor.
* **Breaking Change** Change the DatabaseType to a string rather than an enum (This makes it easier to pass values about with needing a dependency on EntityFrameworkCore.Extensions).
* General source code tidying including standardising regions.
* Replaced Lambda methods with block methods for consistency and ease of adding guard clauses later. 
* **Breaking Change** Replaced `params object[]` parameter in methods with `params IEnumerable<object>`. Removed the existing query methods that took `IQueryable<object>` as the parameters collection as they are now redundant.
* Added new `ExecuteInsert` methods that take a generic parameter and return a key of this type (Kept the existing methods that return a `long` for convienience).
* **Breaking Change** Renamed `ExecuteUpdate` to `ExecuteUpdateGetCount` and added new `ExecuteUpdateGetRows` methods which return the rows updated instead of a count.
* Switched from using a .ruleset file for source analysis to .editorconfig.
* Added new `ExecuteQuery` methods that take a generic entity parameter and return a list of the entities.
* Added new `ExecutePagedQuery` methods that take a generic entity parameter and return a list of the entities in the QueryPage (now PageResultxxxx) object.
* Made generic parameter names in source code consistent:  
 `<T>` = General type.  
 `<TEntity>` = Entity type.  
 `<TProperty>` = Property type (used in execute update code only).  
* Added guard clauses to public methods.
* **Breaking Change** Removed TableExists methods as these did not support schemas and can be easily replicated in standard EF Core.
* Enabled nullable reference types in both projects and corrected any issues associated with this.
* **Breaking Change** Removed `DbContext GetContext<TEntity>(this IQueryable<TEntity> query)` as it required a reflection hack which made it unsupportable between EF versions.
* **Breaking Change** Altered `GetUniqueConstraintDetails` and `GetUniqueConstraintDetailsAsync` to take the database facade as their first parameter.
* Refactored tests to make the results easier to read.

### Version 3.0.2.1 - 08th January 2025

* Fixed issue with GetContext() where it only worked for `DbSet<>` objects and not `IQueryable<>`.
* Upgraded SonarAnalyzer.

### Version 3.0.2.0 - 13th November 2024

* Upgraded solution to .Net 9, Entity Framework Core to 9 and MSTest Nuget packages.

### Version 3.0.1.0 - 05th October 2024

* Upgraded solution to .Net 8, Entity Framework Core to 8.0.8, SonarAnalyzer, StyleCop and MSTest Nuget packages.
* Fixed style checker errors.
* Split test methods into sync and async tests rather than using one method. 
* Made test names more logical.

### Version 3.0.0.0 - 4th June 2023

* Removed Interpolated and Raw suffixes from query method names. For example ExecuteScalarRaw and ExecuteScalarInterpolated are both now known as ExecuteScalar and the compiler chooses the correct method to use based on the parameters (method overloading).
* Upgraded SonarAnalyzer and MSTest Nuget packages.
* Tidied up projects and style checker settings.

### Version 2.0.1.0 - 27th April 2023

* Added new methods to the Execute SQL Extensions that take an `IEnumerable<object>` as a parameter instead of `params object[]`.
* Added new tests for Execute SQL Extensions that take an `IEnumerable<object>`.
* Refactored and tidied up existing test code to make it more consistent.
* Upgraded Entity Framework Core to version 7.0.5 and SonarAnalyzer packages.

### Version 2.0.0.4 - 18th March 2023

* Upgraded Entity Framework Core to version 7.0.4, SonarAnalyzer and MSTest Nuget packages.

### Version 2.0.0.4 - 17th January 2023

* Remove unnecessary generic constraint from GetContext.

### Version 2.0.0.3 - 16th January 2023

* Upgraded Entity Framework Core to version 7.0.2, SonarAnalyzer and MSTest Nuget packages.
* Added new GetContext extension to get the DbContext from an IQueryable<T> object.

### Version 2.0.0.2 - 20th November 2022

* Standardised and removed superfluous message from UniqueConstraintException.

### Version 2.0.0.1 - 12th November 2022

* Added missing cancellationToken parameter to ExecuteUpdateAsync.

### Version 2.0.0.0 - 11th November 2022

* Upgraded solution to .Net 7, Entity Framework Core to 7.0.0.
* Upgraded SonarAnalyzer.
* Added ExecuteUpdate extensions.

### Version 1.0.4.2 - 17th October 2022

* Standardised exception message in UniqueConstraintException.

### Version 1.0.4.1 - 17th October 2022

* Corrected namespace issue that broke existing clients.

### Version 1.0.4.0 - 17th October 2022

* Upgraded Entity Framework Core to version 6.0.10.
* Added interceptor and extensions to capture and process unique constraint exceptions.

### Version 1.0.3.1 - 19th September 2022

* Upgraded Entity Framework Core to version 6.0.9, SonarAnalyzer and MSTest Nuget packages.

### Version 1.0.3.0 - 29th May 2022

* Added extension methods to check the existance of a specified database or table.
* Refactored the private query methods held in QueryExtensions into a new class called QueryMethods and made them internal so they can be called from other extension classes in the project.

### Version 1.0.2.6 - 24th May 2022

* Upgraded Entity Framework Core to version 6.0.5, SonarAnalyzer and MSTest Nuget packages.
* Changed namespaces to be file scoped.

### Version 1.0.2.5 - 7th May 2022

* Upgraded SonarAnalyzer and MSTest Nuget packages.
* Minor code change in QueryExtensions LimitQuery to remove duplicated code.

### Version 1.0.2.4 - 21st April 2022

* Upgraded Entity Framework Core to version 6.0.4, SonarAnalyzer and MSTest Nuget packages.
* Changed project from using <Version> to using <VersionPrefix> for compatibility with new internal packaging script.

### Version 1.0.2.3 - 15th March 2022

* Upgraded Entity Framework Core to version 6.0.3 and SonarAnalyzer Nuget packages.

### Version 1.0.2.2 - 1st March 2022

* Upgraded SonarAnalyzer.CSharp package and tidied up properties in project files.

### Version 1.0.2.1 - 13th February 2022

* Corrected typo in package company name.

### Version 1.0.2.0 - 12th February 2022

* Upgraded Entity Framework Core to version 6.0.2 and Nuget packages used by the tests to the latest versions
* Added Style Checkers and rules
* Turned on Treat Warnings as Errors
* Fixed any errors raised by the Style Checkers

### Version 1.0.1.3 - 27th January 2022

Added GetDatabaseType extension method for MigrationBuilder objects.

### Version 1.0.1.2 - 16th January 2022

* Minor code refactoring.

### Version 1.0.1.1 - 15th January 2022

* Upgraded project so it builds a Nuget package.

### Version 1.0.1.0 - 1st January 2022

* Added support for SQL Server.

### Version 1.0.0.8 - 19th December 2021

* Upgraded Entity Framework Core to version 6.0.1
 
### Version 1.0.0.7 - 28th November 2021

* Upgraded solution to .Net 6, Entity Framework Core to 6.0.0 and Nuget packages used by the tests to the latest versions

### Version 1.0.0.6 - 12th October 2021

* Upgraded Entity Framework Core to version 5.0.11 and Nuget packages used by the tests to the latest versions

### Version 1.0.0.5 - 30th August 2021

* Upgraded Entity Framework Core to version 5.0.9 and Nuget packages used by the tests to the latest versions

### Version 1.0.0.4 - 25th July 2021

* Upgraded Entity Framework Core to version 5.0.8 and Nuget packages used by the tests to the latest versions

### Version 1.0.0.3 - 15th June 2021

* Upgraded Entity Framework Core to version 5.0.7 and Nuget packages used by the tests to the latest versions

### Version 1.0.0.2 - 12th May 2021 

* Upgraded Entity Framework Core to version 5.0.6

### Version 1.0.0.1 - 22nd April 2021 

* Upgraded Nuget packages.

### Version 1.0.0.0 - 05th April 2021 

* Initial release.
