## Running the project
In order to run the project you need to execute the following commands after executing "Sql Scripts.sql" to restore the database.

npm install
npm run build-css
dotnet restore
dontet run

## Integrating the project with your site
In order to integrate the project with your site you need to do the following

1. Make a copy of BoaController.cs and modify the snippets in each of the methods to your own.
2. Create a route for your integration and copy the HTML from BoaIntegration.cshtml into yours.
3. Create two aunthetication routes
	- "Account/BoaLogin"
	- "Account/BoaRegister"
These two will be invoked and shown in a dialog if the user is not logged in and for example tries to place a bet.

Note. in order to reload the intgration iframe from the latter pages make use of the following line of code:
	window.parent.postMessage("BOA_REDIRECT", "");

4. Finally, create two a subdomin to your own website, for example integration.example.com and point it to the ip we provide you with. Use this url in the iframe of the integration.

