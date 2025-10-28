# Documentation

Welcome to the K8s Compliment Shop documentation! This directory contains guides and references to help you work with the .NET microservices in this project.

## For Java Developers

Coming from Java/Spring/Maven? These guides will help you get up to speed with .NET quickly:

### 📘 [.NET for Java Developers](DOTNET_FOR_JAVA_DEVS.md)

A comprehensive comparison guide covering:

- **Project GUIDs**: What are those random alphanumeric strings like `{7F9E3C4D-2A1B-4E5C-9D8A-1B2C3D4E5F6A}`?
- **Project Files**: `pom.xml` → `.csproj` comparison
- **Dependency Management**: Maven Central → NuGet.org
- **Build Commands**: `mvn` → `dotnet` mapping
- **Dependency Injection**: Spring `@Autowired` → Constructor injection
- **Testing**: JUnit/Mockito → xUnit/Moq
- **ORM**: JPA/Hibernate → Entity Framework Core
- **Annotations**: `@Annotation` → `[Attribute]`
- **Async Programming**: `CompletableFuture` → `async/await`
- **Common Gotchas** and tips

**Perfect if:** You need conceptual understanding of .NET coming from Java

---

### 📗 [.NET CLI Cheat Sheet](CHEATSHEET.md)

Quick command reference with Maven equivalents:

| Task | Maven | .NET CLI |
|------|-------|----------|
| Build | `mvn compile` | `dotnet build` |
| Test | `mvn test` | `dotnet test` |
| Run | `mvn spring-boot:run` | `dotnet run` |
| Package | `mvn package` | `dotnet publish` |
| Add dependency | Edit pom.xml | `dotnet add package` |
| Clean | `mvn clean` | `dotnet clean` |

Plus:
- **Entity Framework** commands (like Flyway/Liquibase)
- **Testing workflows** (watch mode, filtering, coverage)
- **Common development patterns**
- **Helper scripts** usage

**Perfect if:** You need quick command lookups while working

---

## Service-Specific Documentation

Each service has its own documentation:

- **[product-service](../services/product-service/)** - Compliment catalog service
  - [Service README](../services/product-service/README.md) - Architecture and usage
  - [Local Testing Guide](../services/product-service/LOCAL_TESTING.md) - Docker testing
  - [Scripts Documentation](../services/product-service/scripts/README.md) - Helper scripts
  - [Test Documentation](../services/product-service/tests/ProductService.Tests/README.md) - Unit tests

## Quick Start

### 1. Read the Comparison Guide (10 min)
```bash
open docs/DOTNET_FOR_JAVA_DEVS.md
```

Learn the key concepts and how they map from Java.

### 2. Keep the Cheat Sheet Handy
```bash
open docs/CHEATSHEET.md
```

Reference this while coding for quick command lookups.

### 3. Try the Helper Scripts
```bash
cd services/product-service

# Build and test
./scripts/build.sh --with-tests

# Run tests in watch mode
./scripts/test.sh --watch

# Run service locally
./scripts/run-local.sh
```

## Common Questions

### Q: What's a `.sln` file?
**A:** It's like a Maven parent `pom.xml` - groups multiple projects together. See: [.NET for Java Developers](DOTNET_FOR_JAVA_DEVS.md#solution-files-multi-module-projects)

### Q: What's that random GUID in the `.sln` file?
**A:** It's a unique project identifier (Visual Studio legacy). You rarely touch these - they're auto-generated. See: [Project GUIDs](DOTNET_FOR_JAVA_DEVS.md#project-guids-whats-that-random-string)

### Q: How do I add a NuGet package?
**A:** `dotnet add package PackageName` (like editing `pom.xml` and running `mvn install`). See: [Cheat Sheet](CHEATSHEET.md#dependency-management)

### Q: Where's the equivalent of `@Autowired`?
**A:** .NET uses constructor injection by default - no annotation needed. See: [Dependency Injection](DOTNET_FOR_JAVA_DEVS.md#dependency-injection)

### Q: How do I run tests in watch mode (like Maven `mvn test -DfailIfNoTests=false`)?
**A:** `dotnet watch test` or use `./scripts/test.sh --watch`. See: [Testing](CHEATSHEET.md#testing)

### Q: What's `async/await`?
**A:** Like Java's `CompletableFuture` but cleaner syntax. See: [Async/Await](DOTNET_FOR_JAVA_DEVS.md#asyncawait-java-vs-net)

## Additional Resources

### Official .NET Documentation
- [Microsoft .NET Docs](https://docs.microsoft.com/en-us/dotnet/)
- [C# Language Tour](https://docs.microsoft.com/en-us/dotnet/csharp/tour-of-csharp/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)

### Tools Documentation
- [xUnit Testing](https://xunit.net/)
- [Moq (Mocking)](https://github.com/moq/moq4)
- [FluentAssertions](https://fluentassertions.com/)
- [gRPC for .NET](https://docs.microsoft.com/en-us/aspnet/core/grpc/)

### Kubernetes
- [Deploy .NET to Kubernetes](https://docs.microsoft.com/en-us/dotnet/architecture/containerized-lifecycle/design-develop-containerized-apps/build-aspnet-core-applications-linux-containers-aks-kubernetes)
- [Health Checks](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)

## Contributing to Docs

Found something unclear or missing? Please add to these docs! Guidelines:

1. **Keep it practical** - Focus on real examples over theory
2. **Show Java equivalents** - Always compare to Java when possible
3. **Use examples** - Code snippets are better than explanations
4. **Keep it current** - This project uses .NET 8 LTS

## Document Index

```
docs/
├── README.md                      (This file)
├── DOTNET_FOR_JAVA_DEVS.md       (Comprehensive Java → .NET guide)
└── CHEATSHEET.md                  (Quick command reference)
```

---

**Happy coding!** 🚀

If you get stuck, the guides above cover 99% of what you'll need. For anything else, the [official .NET docs](https://docs.microsoft.com/en-us/dotnet/) are excellent.
