# PointW.WebApi.ResourceModel

** PLEASE NOTE: support for HAL is pretty stable, but support for Collection+JSON is still in flight **

The goal of this project is to provide a simple way to model resources.  The essential considerations are
* what data should a resource provide in its representations
* how are resources related to each other
* leave formatting decisions to configuration (as much as possible)

This should be easy to model as simple classes without having to consider what the format of representations actually look like.

(see the <a href=https://github.com/biscuit314/PointW.WebApi.ResourceModel/wiki>wiki</a> for more details.)

## Getting Started
The following walks through the evolution of a very simple resource model.  This will demonstrate how this project improves already cool out of the box stuff that Web Api comes with.  This is just an illustration, with lots of details like database access, exception handling left out.

Our goal:  when the system receives a GET request for a Person resource, we want to respond with a representation formatted according to the HAL specification.  For example, if John Doe has a Mustang, we may see get a representation like this:

	{
	  "name": "John Doe",
	  "address": "123 Main St.",
	  "_links": {
		"car": {
		  "href": "http://example.org/api/car/1"
		},
		"self": {
		  "href": "http://example.org/api/person/1"
		}
	  }
	}

Following the "car" link relation will give us details about John's car, which happens to be a Mustang.

That's where we're going.  Let's get there step by step.

### People who love people
Let's start really simply.  I've got one resource **Person**, modelled as a `Person` class:

	public class Person
	{
		public string Name { get; set; }
		public string Address { get; set; }
		public string Phone { get; set; }
	}

and one controller `PersonController` which has one method `Get()`

    public class PersonController : ApiController
    {
        private Dictionary<int, Person> _people = new Dictionary<int, Person>
        {
            { 1, new Person { Name = "John Doe", Address = "123 Main St." } },
            { 2, new Person { Name = "Sally Smith", Address = "321 Main St.", Phone = "555-123-4567"} }
        };
            
        [Route("api/person/{personId}", Name = "GetPersonById")]
        public IHttpActionResult Get(int personId)
        {
            if (! _people.ContainsKey(personId))
            {
                return NotFound();
            }

            var person = _people[personId];
            return Ok(person);
        }
    }


I run this, do a GET request on `/api/person/1`, which gives the following:

    {"Name":"John Doe","Address":"123 Main St.","Phone":null}

Nice, but there are a couple observations:
* JSON is traditionally camelCase.  I wish it was "name":"John Doe"
* This person doesn't have a phone.  I would like to omit null fields.
* How do I provide link relations?

### Baby you can drive my car
Moving on... A person may own a car (just one for now to keep it simple).  So I model a **Car** resource as a `Car` class, like this:

	public class Car
	{
		public string Make { get; set; }
		public string Model { get; set; }
		public string SerialNumber { get; set; }
	}

and (again to keep it simple) here is the `CarController`

    public class CarController : ApiController
    {
        private Dictionary<int, Car> _cars = new Dictionary<int, Car>
        {
            { 1, new Car { Make = "Ford", Model = "Mustang" } },
            { 2, new Car { Make = "Chevrolet", Model = "Corvette"} }
        };

        [Route("api/car/{carId}", Name = "GetCarById")]
        public IHttpActionResult Get(int carId)
        {
            if (!_cars.ContainsKey(carId))
            {
                return NotFound();
            }

            return Ok(_cars[carId]);
        }
    }

I can now add car ownership to the `Person` class:

    public class Person
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int MyCarId { get; set; }
    }

And for the sake of this demo, adjust the `PersonController` dictionary:

    private Dictionary<int, Person> _people = new Dictionary<int, Person>
    {
        { 1, new Person { Name = "John Doe", Address = "123 Main St.", MyCarId = 1 } },
        { 2, new Person { Name = "Sally Smith", Address = "321 Main St.", Phone="555-123-4567", MyCarId = 2} }
    };

Predictably, doing a GET `/api/person/1` now looks like this:

    {"Name":"John Doe","Address":"123 Main St.","Phone":null,"MyCarId":1}

## You can call me HAL
For now this project contains only one media type formatter: <a href="http://stateless.co/hal_specification.html">HAL</a>  Let's see how we can make the **Person** / **Car** stream out representations in the HAL format with link relations.

After adding `PointW.WebApi.ResourceModel` to your Web Api project, add the HAL formatter to your config section.

    config.MapHttpAttributeRoutes();
    
    config.Formatters.Clear();
    config.Formatters.Add(new HalJsonMediaTypeFormatter { Indent = true });
    
    config.EnsureInitialized();

_(Note: in practice I set Indent = true only #if DEBUG)_

Now have the `Person` and `Car` classes inherit from `Resource` (be sure to add `using PointW.WebApi.ResourceModel;`)

    public class Person : Resource
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int MyCarId { get; set; }
    }

...

    public class Car : Resource
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
    }

When we deliver a **Person** resource, we want the representation to contain, not the ID of the car the person owns, but a _link_ so the client can GET the **Car** resource.  This is easy.  In the `PersonController` class, change the Get method as follows:

    [Route("api/person/{personId}", Name = "GetPersonById")]
    public IHttpActionResult Get(int personId)
    {
        if (! _people.ContainsKey(personId))
        {
            return NotFound();
        }
    
        var person = _people[personId];
        
        person.Relations.Add("car", 
            new Link
            {
                Href = Url.Link("GetCarById", new { carId = person.MyCarId })
            });
        
        return Ok(person);
    }
        
The heart of the Resource model is the `Relations` property.  Each Resource can have a list of other resources related to it.  In this case a **Person** is related to a **Car**, and we have just added a link from **Person** to the resource of **Car** she owns.

Now when GET `api/person/1` this is what we receive:

    {
      "name": "John Doe",
      "address": "123 Main St.",
      "myCarId": 1,
      "_links": {
        "car": {
          "href": "http://example.org/api/car/1"
        }
      }
    }

This is much nicer:
* JSON is in camelCase
* representation is in HAL format
* null values do not appear in the representation (you can override this with the [AlwaysShow] attribute)
* The client can navigate from this resource to the **Car** resource by dereferencing the "car" link relation

We can tidy things up a bit:  We don't really need "myCarId" to appear in the representation.  We can model this by applying an attribute to that property:

    public class Person : Resource
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        [NeverShow]
        public int MyCarId { get; set; }
    }

Then we should add a "self" link relation to the Get() method of the PersonController:

    person.Relations.Add("self",
        new Link
        {
            Href = Url.Link("GetPersonById", new { personId = personId })
        });

GET `/api/person/1` and see that we have arrived at our goal:

	{
	  "name": "John Doe",
	  "address": "123 Main St.",
	  "_links": {
		"car": {
		  "href": "http://example.org/api/car/1"
		},
		"self": {
		  "href": "http://example.org/api/person/1"
		}
      }
	}
