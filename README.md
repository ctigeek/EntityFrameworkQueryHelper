# EntityFrameworkQueryHelper
Build queries based on parameters passed in from an MVC controller (or other source).

## What is this? 

I built this little framework because I wanted to be able to query data via a REST API and Entity Framework.
In the example code, the parameters comes from the URL query string of a WebAPI request.
The parameters are used by the Query Helper to create a LINQ expression tree which can be executed by EF.

If you want to look at the code, start with the Controller and work your way down.

Here are some example API calls based on the example code.

Get the first ten records:

    http://myapi.com/MyController/?offset=0&limit=10

Get the next ten records:

    http://myapi.com/MyController/?offset=10&limit=10

Get all the records for October 1st: (multiple where statements are separated by commas.)

    http://myapi.com/MyController/?where=TimeStamp>=2015-10-01T00:00:00,TimeStamp<=2015-10-01T23:59:59

Same query, but order by TimeStamp in descending order:
	
    http://myapi.com/MyController/?where=TimeStamp>=2015-10-01T00:00:00,TimeStamp<=2015-10-01T23:59:59&orderby=TimeStamp&orderdesc=true

Where statements have to be exact, but you can also specify a Search statement to find partial strings.
	
    http://myapi.com/MyController/?where=name=bob&search=description~logged in
	
Notice that where statements can use '>=','>','<','<=','=','!=', and work with any data type.

However search statements use '~' as the operator and can only operate on strings.