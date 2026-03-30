---
name: java-legacy-analysis
description: >-
  Analyzes legacy Java applications (Java EE, Servlets, JSP, EJB, Spring) for
  migration readiness. Use when analyzing Java code structure, identifying
  framework-specific patterns, or understanding legacy Java architectures.
---

## Purpose

Provide specialized knowledge for analyzing legacy Java applications to support migration planning and business logic extraction.

## Application Type Detection

| Type | Key Indicators |
|------|---------------|
| **Servlet/JSP** | `web.xml`, `HttpServlet`, `doGet`/`doPost`, `*.jsp` files |
| **EJB** | `@Stateless`, `@Stateful`, `@MessageDriven`, `ejb-jar.xml` |
| **Spring (legacy)** | `applicationContext.xml`, `@Controller`, `@Service`, `@Repository` |
| **Spring Boot** | `@SpringBootApplication`, `application.properties`, `pom.xml` with spring-boot-starter |
| **Struts** | `struts-config.xml`, `Action` classes, `ActionForm` |
| **JSF** | `faces-config.xml`, `@ManagedBean`, `*.xhtml` Facelets |

## Java Version Detection

- Check `pom.xml` for `<maven.compiler.source>` and `<maven.compiler.target>`
- Check `build.gradle` for `sourceCompatibility` and `targetCompatibility`
- Look for `MANIFEST.MF` entries
- Identify Java-version-specific APIs in use

## Key Analysis Areas

### Dependency Injection
- **Spring XML**: `<bean>` definitions in XML config
- **Spring Annotations**: `@Autowired`, `@Component`, `@Service`
- **CDI (Java EE)**: `@Inject`, `@Named`, `beans.xml`
- **EJB**: `@EJB` injection

### Data Access Patterns
- **JDBC direct**: `DriverManager.getConnection`, `PreparedStatement`
- **JPA/Hibernate**: `@Entity`, `EntityManager`, `@PersistenceContext`
- **Spring Data**: `JpaRepository`, `CrudRepository` interfaces
- **MyBatis**: `@Mapper`, XML SQL mappings

### Web Service Patterns
- **SOAP**: `@WebService`, `@WebMethod`, WSDL files
- **JAX-RS (REST)**: `@Path`, `@GET`, `@POST`, `@Produces`
- **Spring MVC REST**: `@RestController`, `@RequestMapping`

### Authentication
- **JAAS**: `LoginModule`, `jaas.conf`
- **Spring Security**: `WebSecurityConfigurerAdapter`, `@Secured`
- **Container-managed**: `web.xml` security constraints, `@RolesAllowed`

### Configuration Sources
- `application.properties` / `application.yml` (Spring)
- `web.xml` (Servlet config)
- JNDI lookups (`InitialContext.lookup`)
- System properties and environment variables

## Migration Target Recommendations

| Legacy Source | Recommended Target | Rationale |
|--------------|-------------------|-----------|
| Servlets/JSP | Spring Boot MVC | Modern, well-supported, familiar patterns |
| EJB | Spring Boot services | CDI-like DI, simpler deployment |
| Struts | Spring Boot MVC | Direct action-to-controller mapping |
| JSF | Spring Boot + Thymeleaf or React | Server-side or SPA depending on complexity |
| SOAP services | Spring Boot REST | RESTful APIs are the modern standard |
| Java EE 7/8 | Jakarta EE 10+ or Spring Boot 3 | Either path viable, Spring Boot more common |
