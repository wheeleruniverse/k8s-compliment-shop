# .NET for Java Developers

A quick reference guide for Java developers learning .NET/C#.

## Project GUIDs: What's That Random String?

### The String You Asked About

```
{7F9E3C4D-2A1B-4E5C-9D8A-1B2C3D4E5F6A}
```

This is a **Project GUID** (Globally Unique Identifier) used in Visual Studio solution files.

### Java vs .NET Project Identity

| Java (Maven/Gradle) | .NET (MSBuild/dotnet) |
|---|---|
| `groupId:artifactId:version` | Project GUID in .sln file |
| Example: `com.example:my-app:1.0.0` | Example: `{GUID}` |
| Human-readable coordinates | Machine-generated identifier |
| Defined in `pom.xml` or `build.gradle` | Defined in `.sln` file |

### Why GUIDs?

**Visual Studio Legacy**: GUIDs uniquely identify projects in a solution, enabling:
- Project references in multi-project solutions
- Build dependencies and ordering
- IDE workspace management

**Good News**: You rarely touch these! They're auto-generated and only visible in `.sln` files.

### When You'll See GUIDs

```xml
<!-- ProductService.sln -->
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ProductService", "src\ProductService\ProductService.csproj", "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}"
EndProject
```

- First GUID: Project type (C# project)
- Second GUID: Unique project identifier

### Modern .NET CLI

**You don't need to worry about GUIDs!** The `dotnet` CLI handles everything:

```bash
# Java
mvn archetype:generate

# .NET (no GUIDs needed!)
dotnet new webapi -n MyService
```

## Project Files: pom.xml → .csproj

### Side-by-Side Comparison

**Java (pom.xml):**
```xml
<project>
  <groupId>com.example</groupId>
  <artifactId>product-service</artifactId>
  <version>1.0.0</version>

  <dependencies>
    <dependency>
      <groupId>org.springframework.boot</groupId>
      <artifactId>spring-boot-starter-web</artifactId>
      <version>3.1.0</version>
    </dependency>
  </dependencies>
</project>
```

**.NET (.csproj):**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Grpc.AspNetCore" Version="2.59.0" />
  </ItemGroup>
</Project>
```

**Key Differences:**
- .NET project files are **much shorter** (SDK handles defaults)
- No `groupId` - packages use single identifier
- `TargetFramework` instead of Java version

## Solution Files: Multi-Module Projects

### Java (Maven)

```xml
<!-- Parent pom.xml -->
<modules>
  <module>product-service</module>
  <module>order-service</module>
</modules>
```

### .NET (Solution)

```
ProductService.sln  ← Solution file (like parent pom.xml)
├── ProductService.csproj
└── ProductService.Tests.csproj
```

**Create solution:**
```bash
# Java
# (edit parent pom.xml manually)

# .NET
dotnet new sln -n ProductService
dotnet sln add src/ProductService/ProductService.csproj
dotnet sln add tests/ProductService.Tests/ProductService.Tests.csproj
```

## Package Management

| Java | .NET |
|---|---|
| Maven Central | NuGet.org |
| `<dependencies>` in pom.xml | `<PackageReference>` in .csproj |
| `mvn install` | `dotnet restore` |
| `~/.m2/repository` | `~/.nuget/packages` |

**Adding Dependencies:**

```bash
# Java
# (edit pom.xml manually or use IDE)

# .NET
dotnet add package Newtonsoft.Json
dotnet add package xunit --version 2.6.2
```

## Build & Test Commands

| Task | Java (Maven) | .NET (dotnet CLI) |
|---|---|---|
| **Clean** | `mvn clean` | `dotnet clean` |
| **Restore Dependencies** | `mvn install` (downloads) | `dotnet restore` |
| **Compile** | `mvn compile` | `dotnet build` |
| **Run Tests** | `mvn test` | `dotnet test` |
| **Package** | `mvn package` | `dotnet publish` |
| **Run App** | `mvn spring-boot:run` | `dotnet run` |
| **Full Build** | `mvn clean install` | `dotnet build` |

## Project Structure

### Java (Spring Boot)
```
src/
├── main/
│   ├── java/com/example/product/
│   │   ├── ProductController.java
│   │   ├── ProductService.java
│   │   └── ProductRepository.java
│   └── resources/
│       └── application.properties
└── test/
    └── java/com/example/product/
        └── ProductServiceTest.java
```

### .NET
```
src/ProductService/
├── Controllers/ (or Services/ for gRPC)
│   └── ProductGrpcService.cs
├── Models/
│   └── Product.cs
├── Repositories/
│   └── ProductRepository.cs
├── Program.cs  ← Entry point (like @SpringBootApplication)
└── appsettings.json  ← application.properties equivalent

tests/ProductService.Tests/
└── Unit/
    └── ProductServiceTests.cs
```

## Dependency Injection

### Java (Spring)
```java
@Service
public class ProductService {
    @Autowired
    private ProductRepository repository;
}
```

### .NET
```csharp
// Program.cs - Registration
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Service - Constructor injection
public class ProductService {
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository) {
        _repository = repository;
    }
}
```

**Key Differences:**
- .NET uses **constructor injection** (no `@Autowired`)
- Explicit registration in `Program.cs` (no classpath scanning)
- Interfaces are conventional (not required, but best practice)

## Testing Frameworks

| Java | .NET |
|---|---|
| JUnit 5 | xUnit / NUnit / MSTest |
| Mockito | Moq |
| AssertJ | FluentAssertions |
| `@Test` | `[Fact]` or `[Theory]` |

### Java (JUnit + Mockito)
```java
@Test
void testGetProduct() {
    when(repository.findById(1)).thenReturn(Optional.of(product));

    Product result = service.getProduct(1);

    assertThat(result.getName()).isEqualTo("Test");
}
```

### .NET (xUnit + Moq + FluentAssertions)
```csharp
[Fact]
public async Task GetProduct_ReturnsProduct() {
    _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

    var result = await _service.GetProduct(1);

    result.Name.Should().Be("Test");
}
```

## Annotations vs Attributes

| Java | .NET | Purpose |
|---|---|---|
| `@Service` | `[Service]` (not needed) | DI container registration |
| `@Entity` | `[Table]` | ORM mapping |
| `@Test` | `[Fact]` | Unit test |
| `@RequestMapping` | `[Route]` | HTTP routing |
| `@Autowired` | N/A | Constructor injection |

**.NET Syntax:**
```csharp
[Table("Products")]  // ← Square brackets, not @
public class Product { }
```

## Naming Conventions

| Java | .NET |
|---|---|
| `productService` (camelCase) | `_productService` (fields with `_` prefix) |
| `ProductService` (PascalCase) | `ProductService` (PascalCase) |
| `getProductById()` | `GetProductByIdAsync()` |
| `CONSTANT_VALUE` | `ConstantValue` or `CONSTANT_VALUE` |

## Async/Await (Java vs .NET)

### Java (CompletableFuture)
```java
public CompletableFuture<Product> getProductAsync(int id) {
    return CompletableFuture.supplyAsync(() ->
        repository.findById(id)
    );
}
```

### .NET (async/await)
```csharp
public async Task<Product> GetProductAsync(int id) {
    return await _repository.GetByIdAsync(id);
}
```

**Key Differences:**
- .NET's `async/await` is **syntactic sugar** (cleaner than Java)
- `Task<T>` ≈ `CompletableFuture<T>`
- Methods end with `Async` by convention

## ORM: JPA/Hibernate → Entity Framework Core

| JPA/Hibernate | Entity Framework Core |
|---|---|
| `@Entity` | No annotation needed |
| `@Table(name="products")` | `[Table("Products")]` or Fluent API |
| `@Id` | `[Key]` |
| `@GeneratedValue` | `[DatabaseGenerated]` |
| `EntityManager` | `DbContext` |
| `repository.findById()` | `context.Products.FindAsync()` |

### Java (JPA)
```java
@Entity
@Table(name = "products")
public class Product {
    @Id
    @GeneratedValue
    private Long id;
}
```

### .NET (EF Core)
```csharp
public class Product {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
}
```

## Configuration Files

| Java | .NET |
|---|---|
| `application.properties` | `appsettings.json` |
| `application.yml` | `appsettings.Development.json` |

### Java
```properties
server.port=8080
spring.datasource.url=jdbc:mysql://localhost:3306/db
```

### .NET
```json
{
  "Kestrel": {
    "Endpoints": { "Http": { "Url": "http://+:8080" } }
  },
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=db"
  }
}
```

## Quick Tips

1. **No Lombok needed** - C# has built-in properties:
   ```csharp
   public string Name { get; set; }  // Auto-implemented property
   ```

2. **LINQ is amazing** - Like Java Streams but more powerful:
   ```csharp
   var results = products
       .Where(p => p.Category == "Test")
       .OrderBy(p => p.Name)
       .ToList();
   ```

3. **Nullable Reference Types** - Like Java's `@Nullable`:
   ```csharp
   string? nullableName;   // Can be null
   string nonNullName;     // Cannot be null (compiler enforced)
   ```

4. **Records** - Immutable data classes (like Java records):
   ```csharp
   public record ProductDto(int Id, string Name);
   ```

## Common Gotchas

1. **Case Sensitivity**: C# is case-sensitive, JSON binding is case-insensitive by default
2. **Value vs Reference Types**: `int`, `bool` are structs (stack), `string`, objects are classes (heap)
3. **Dispose Pattern**: Use `using` blocks instead of `try-finally`:
   ```csharp
   using var context = new DbContext();  // Auto-disposed
   ```

## Resources

- [Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/)
- [C# from Java](https://learn.microsoft.com/en-us/dotnet/csharp/tour-of-csharp/program-building-blocks)
- [EF Core vs JPA](https://learn.microsoft.com/en-us/ef/core/)

---

**Bottom Line**: .NET and Java are very similar! Most concepts translate directly, just with different syntax and conventions.
