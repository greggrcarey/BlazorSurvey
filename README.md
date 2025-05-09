# Blazor Survey

After leaving my last employer, I wanted to see current versions of .NET and ASP.NET. All of my previous clients used .NET Framework, which hasn't been recommended for new development since before 2019. I knew I needed to sharpen up on the newest technology, so I studied Apps and Services with .NET 8  by Mark J Price. Studying out of a book is great, but to really learn with a book, you have to build something with it.

## Requirements

The Blazor Survey is a web app that lets users create surveys and share them with others. Users can take surveys anonymously. Survey creators can then see an aggregated roll up of survey result. Finally, Surveys can be deleted by survey creator if needed. Surveys cannot be updated after being created since survey participants would see different versions depending on when they take a survey. 

Really these requirements are a test bed for the technology I wanted to explore. 
- CosmosDB as a backing datastore
- System.Text.Json library for JSON manipulation and for using it new polymorphic capabilities
- Minimal API to build the REST API
- OTEL for logging 
- ASP.NET Identity for cookie based authentication/ authorization
	- I also needed a SQL Server database for Identity
- Blazor Web App with Interactive Auto rendering 
- MudBlazor for ready made Blazor controls
- Deploy to Azure

## Lessons Learned
 
This project did take me longer than I expected, but I was also using tools I had never used before and trying my best to use.
- When I started using CosmosDB, EF core didn't support polymorphism with System.Text.Json, so I used the CosmosDB Client directly. 
- I thought polymorphism was going to be great when it came to structuring my model, but anything more complex than what I have now would get out of hand. My model really just contains meta data at the base class level, with more specific properties in child classes. 
- The CosmosDB document model converged on a single document that contains all questions and responses. This "schema" is cheap from a CosmosDB perspective since it allows point reads for accessing documents. The results are calculated by a Stored Procedure on Cosmos to have the most up to date data possible. 
- I enjoy Minimal APIs, but as of .NET 9, you don't get validation for free like in Controller based APIs, so that needs to be accounted for. Seems like model validation for Minimal APIs is coming in .NET 10.
- I have only worked on Single Page Applications using JavaScript frameworks for everything but my first applications in coding bootcamp, so that is how I originally approached Blazor with InteractiveAuto rendering. I was excited when I heard about InteractiveAuto rendering in Blazor because I thought I would just have to add that render mode in the `App.razor` file and be done with it. What you are actually signing up with in InteractiveAuto is managing two application frontends instead of one.
- Only later did I realize that my Cosmos service should run in the Blazor Server project instead of behind it's own Minimal API project. But then this isn't quite accurate either, since the WebAssembly client needs access to the same Cosmos resources. One of the Microsoft Approved Way to handle this is create an Interface that is implemented in both the Server and Client project. This solves the issue, but now there is some code duplication as you have to make sure the interface is implemented the same across projects. I handled this by making the implementation as light as possible, but I see this approach getting out of hand if you aren't careful. The InteractiveAuto render mode is in service of having the best of both Server rendering, which can be interactive as soon as it loads in the browser, and WebAssembly, which allows developers to write C# and users to have a native browser experience when using the application. There are some rough edges that have to be sorted out when Blazor prerenders the page content, but that can be handled with `PersistingComponentStateSubscription` and that developer experience should get better in .NET 10. 

## Goals for next project

I want to take the lessons learned here and build on them with the next project. 
- Add validation to the requests coming from the frontend. Since the frontend can be run from the server or WebAssembly in InteractiveAuto mode, where does it make sense to validate user data? 
- Add a thicker domain model. This project didn't have much logic, but the next one should. 
- Make sure that my Http Responses are correct. 
- Create over inheritance. I mentioned earlier that I could see how cumbersome polymorphic types could become. Researching Creation over Inheritance seems like a way forward.
- `Result<T>` pattern. Avoiding `null` has been a years long battle in C# and with the languages turn towards functional programming, this will be a helpful pattern to explore.
- AI Tools - of course.
- Tests - I completely blew tests off with this project. 
- Better logging - I added some OTEL hooks to this project, but I need to understand better what should happen with logging going in the next project.
