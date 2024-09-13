# MyMDb

MyMDb stands for My Movie Database. It is a project that includes a web application with a .NET API and a React client.

This web app is designed for administrators to add movies or series that are not available on any streaming services, and for users to watch them from anywhere. Users can also leave reviews to help their friends decide whether they should watch the content or not.

# Frontend

The site has a very intuitive user interface. It features a logo that directs to the main page and a login button in the navigation bar. Immediately below that is the library section, which has a centered triple button for filtering media types and the results listed responsively based on screen dimensions.

![MyMDb Main Page](images/mainPage.png?raw=true)

# Backend

The backend consists of a Microsoft SQL Server Database with entities and relationships, as illustrated in the accompanying image (the Identity-related elements are not depicted in the diagram). It is controlled by a dotnet API developed in C#. The API utilizes Entity Framework, Identity Framework, Jwt Bearer, and AutoMapper.

![MyMDb ER Diagram](images/ER_Diagram.jpg?raw=true)
