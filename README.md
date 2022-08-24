# .Net-6-WebAPI-Identity-Demo
.Net 6  WebAPI demo project with JWT Authentiation, Twillio SendGrid Email API ect.

## Tools & Technologies
<ul>
<li>Visual Studio</li>
<li>.Net 6 Web API</li>
<li>Microsoft AspNet Core Identity (JWT Authentication)</li>
<li>Swagger</li>
<li>Serilog</li>
<li>Twillio SendGrid Email API</li>
</ul>

## Summary
This is the .Net 6 WebAPI demo application. It includes the following things.

<ul>
<li>Swagger for testing the API endpoints. Also ignore some of the Http points from showing on the Swagger UI screen.</li>
<li>Implemented the Global Exception Handling. Log the errors in a file using the Serilog Logger.</li>
<li>JWT token based Authentication.</li>
<li>Send Confirmation Email when registering the user. User will not be able to login unless confirmed the email.</li>
<li>Forgot Password email will be sent to user email and the link inside the email will redirect to the New Password screen.</li>
<li>Returning the Weather Forecast list only when the user token is authorized.</li>
</ul>
