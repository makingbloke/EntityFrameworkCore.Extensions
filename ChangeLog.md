### Version 1.0.3.0 - 29th May 2022

* Added extension methods to check the existance of a specified database or table.
* Refactored the private query methods held in QueryExtensions into a new class called QueryMethods and made them internal so they can be called from other extension classes in the project.

### Version 1.0.2.6 - 24th May 2022

* Updated Entity Framework Core to version 6.0.5, SonarAnalyzer and MSTest Nuget packages.
* Changed namespaces to be file scoped.

### Version 1.0.2.5 - 7th May 2022

* Updated SonarAnalyzer and MSTest Nuget packages.
* Minor code change in QueryExtensions LimitQuery to remove duplicated code.

### Version 1.0.2.4 - 21st April 2022

* Updated Entity Framework Core to version 6.0.4, SonarAnalyzer and MSTest Nuget packages.
* Changed project from using <Version> to using <VersionPrefix> for compatibility with new internal packaging script.

### Version 1.0.2.3 - 15th March 2022

* Updated Entity Framework Core to version 6.0.3 and SonarAnalyzer Nuget packages.

### Version 1.0.2.2 - 1st March 2022

* Updated SonarAnalyzer.CSharp package and tidied up properties in project files.

### Version 1.0.2.1 - 13th February 2022

* Corrected typo in package company name.

### Version 1.0.2.0 - 12th February 2022

* Updated Entity Framework Core to version 6.0.2 and Nuget packages used by the tests to the latest versions
* Added Style Checkers and rules
* Turned on Treat Warnings as Errors
* Fixed any errors raised by the Style Checkers

### Version 1.0.1.3 - 27th January 2022

Added GetDatabaseType extension method for MigrationBuilder objects.

### Version 1.0.1.2 - 16th January 2022

* Minor code refactoring.

### Version 1.0.1.1 - 15th January 2022

* Updated project so it builds a Nuget package.

### Version 1.0.1.0 - 1st January 2022

* Added support for SQL Server.

### Version 1.0.0.8 - 19th December 2021

* Updated Entity Framework Core to version 6.0.1
 
### Version 1.0.0.7 - 28th November 2021

* Updated whole project to .Net 6, Entity Framework Core to 6.0.0 and Nuget packages used by the tests to the latest versions

### Version 1.0.0.6 - 12th October 2021

* Updated Entity Framework Core to version 5.0.11 and Nuget packages used by the tests to the latest versions

### Version 1.0.0.5 - 30th August 2021

* Updated Entity Framework Core to version 5.0.9 and Nuget packages used by the tests to the latest versions

### Version 1.0.0.4 - 25th July 2021

* Updated Entity Framework Core to version 5.0.8 and Nuget packages used by the tests to the latest versions

### Version 1.0.0.3 - 15th June 2021

* Updated Entity Framework Core to version 5.0.7 and Nuget packages used by the tests to the latest versions

### Version 1.0.0.2 - 12th May 2021 

* Updated Entity Framework Core to version 5.0.6

### Version 1.0.0.1 - 22nd April 2021 

* Updated Nuget packages.

### Version 1.0.0.0 - 05th April 2021 

* Initial release.
