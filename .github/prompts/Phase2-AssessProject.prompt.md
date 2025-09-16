---
mode: agent
model: Claude Sonnet 3.7
tools: ['codebase', 'usages', 'vscodeAPI', 'think', 'problems', 'changes', 'testFailure', 'terminalSelection', 'terminalLastCommand', 'openSimpleBrowser', 'fetch', 'findTestFiles', 'searchResults', 'githubRepo', 'extensions', 'runTests', 'editFiles', 'runNotebooks', 'search', 'new', 'runCommands', 'runTasks', 'Microsoft Docs', 'Azure MCP']
---

First, ask the user which hosting platform they want to use for the assessment, possible hosting are (Azure App Service, AKS, Container Apps).

Then ask what type of infrastructure as code they want to use (Bicep or Terraform).

Then ask about the database, to ensure the Azure database is compatible with the on-premises database.

If the user does not provide a database, suggest Azure SQL Database (for transactional, relational workloads) or Azure Cosmos DB (for globally distributed, NoSQL, or high-throughput workloads) based on the current application's data access patterns and requirements.

JUST CONTINUE THIS PROMPT IF THE USER CONFIRMS THE HOSTING PLATFORM, INFRASTRUCTURE AS CODE TYPE, AND DATABASE.

In the final answer provide a risk assessment based on the current application versus the target architecture.

# Rules for Assessment of Application
- Then if the workspace does not contain a 'reports' folder, create one.
- Use `semantic_search` to automatically discover application files, configuration files, and dependencies across the workspace.
- Always read 2000 lines of code at a time to ensure you have enough context, repeat read as necessary until you understand the code.
- Use `file_search` to locate specific framework files (*.csproj, pom.xml, package.json, web.config, etc.).
- Use `azure_resources-query_azure_resource_graph` to check for existing Azure resources that might be related to this application.
- Then, assess the workspace and generate a report in the 'reports' folder named 'Application-Assessment-Report.md'.
- Analyze the application to determine if it's a .NET or Java application and identify the current framework version.
- Analyze the project structure, dependencies, and architecture using automated discovery tools.
- Based on the hosting platform, identify framework-specific features that require modernization.
- For .NET applications:
  - Use `grep_search` to find project files (*.csproj, *.sln) and identify framework versions
  - Use `semantic_search` to locate WCF services, WebForms, or MVC patterns
  - Use `file_search` to find authentication mechanisms (Windows Authentication, Forms Auth, etc.)
  - Use `grep_search` to analyze configuration files (web.config, app.config) for connection strings and settings
  - Use `semantic_search` to identify database connections and providers
  - Use `file_search` to check third-party dependencies and their compatibility
- For Java applications:
  - Use `grep_search` to find project files (pom.xml, build.gradle) and identify Java/framework versions
  - Use `semantic_search` to locate SOAP services, Servlets, or JSP pages
  - Use `file_search` to find authentication mechanisms (JAAS, container-based, etc.)
  - Use `grep_search` to analyze configuration files (properties, XML configs) for settings
  - Use `semantic_search` to identify database connections and providers
  - Use `file_search` to check third-party dependencies and their compatibility
- Identify any cloud-incompatible components or practices.
- Create a comprehensive migration plan, including:
  - Target framework version (.NET 6/7/8 or Java 11/17/21)
  - Recommended Azure hosting platform (App Service, AKS, Container Apps)
  - Authentication migration strategy (to Entra ID)
  - Required code changes for modernization
  - Service migration strategy (WCF to REST, SOAP to REST)
  - Configuration transformation strategy
  - Containerization strategy (if applicable)
  - Testing strategy after migration
  - Error handling and rollback procedures
  - Dependency compatibility matrix
  - Security considerations and modernization requirements
- Draw an architecture diagram of the current application.
- Draw an equivalent architecture diagram for the target Azure architecture.
- Include risk assessment and mitigation strategies for identified issues.
- Provide estimated effort and timeline for each migration phase.
- At the end of the "Application-Assessment-Report.md" create a header named "Change Report"
- During the assessment, if a change is required and can or cannot be executed on, include this information into the "Application-Assessment-Report.md": 
- Refactor the code to address [INSERT ISSUE OR BREAKING CHANGE], ensuring compliance with [INSERT FRAMEWORK/VERSION] guidelines
- Reference Supporting Documentation [INSERT LINK TO OFFICIAL DOCUMENTATION]
- Objective[INSERT GOAL — e.g., improve security, remove deprecated API usage, enhance performance]
- Constraints [INSERT CONSTRAINTS — e.g., maintain backward compatibility, avoid performance regressions]
- If not confident in the result, flag the task for review
- If assessment fails due to insufficient information, provide specific guidance on what additional information is needed.
- Before suggesting or applying code changes:
- **Verify the change produces the intended result**
  - Include relevant standards and constraints:
   - Performance
   - Security
   - Readability
   - Maintainability
- **Do not modify code** unless the change can be confidently verified
- Instead, explain what additional information, context, or testing is required to safely implement the change
- If the user runs assess again, ask the user if they want to overwrite the existing report. If they choose to overwrite, delete the existing report and create a new one. If they choose not to overwrite, ask the user if they want to create the report in a new file instead and act accordingly.
- Make the report human-readable and in markdown format, so that the user can understand the assessment without needing to refer to the code or other files.
- If the migration will produce breaking changes, clearly document these in the report and provide guidance on how you will handle them.
- Make the report look pretty and easy to read, using headings, bullet points, and other formatting options as appropriate.
- Include date and time at the beginning of the report.
- Suggest that the next step is to migrate the application code, and mention `/phase3-migratecode` is the command to start the migration process.
- At the end, update the status report file reports/Report-Status.md with the status of the assessment step.



