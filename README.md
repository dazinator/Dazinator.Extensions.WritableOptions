## Updatable Options

Allows you to Update an `IOptions<T>` provided value at runtime. Currently only JSON file support is provided.

`Program.cs`

```

   webBuilder.ConfigureAppConfiguration((b, c) =>
   {
       c.AddJsonFile("mysettings.json", true, true);
	  
```


`Startup.cs`:

```

     services.AddOptions();
	 services.ConfigureJsonUpdatableOptions<TestOptions>("foo:bar", () => File.OpenRead("mysettings.json"), () => File.OpenWrite("mysettings.json"), leaveOpen: false);
	

```

The overload used above lets you specify your own delegate for providing the Read and Write Streams for reading and writing the JSON file.
However if you just want to use System.IO you can use:

```
     services.ConfigureJsonUpdatableOptions<TestOptions>("foo:bar", new FileJsonStreamProvider<TestOptions>("C:/SettingsFolder", "/mysettings.json"));
	 
```

You can now use and update options by injecting `IUpdatableOptions<TestOptions>` like so:


```
public class SomeController
{

    public SomeController(IOptionsUpdater<TestOptions> updater, IOptionsSnapshot<TestOptions> snapShot)
	{
	
	   updater.Update((options)=>{options.SomeFlag = true; }, snapShot);

	   // The "foo:bar" section of the "mysettings.json" file has now been updated.
	}

}
```

Note that when you call `services.ConfigureJsonUpdatableOptions<TOptions>()` or any of it's overloads,
 it will internally call `services.Configure<TOptions>(configuration);` on the `IConfiguration`, or `IConfigurationSection` you provide. This means
 your Options class is set up with the options system, so you can also use it normally in cases where you don't need to update it, by injecting the normal `IOptionsSnapshot<TOptions>' etc.

 ## Updating named options

 Use the `Update` overload that accepts the name for the named options. In this case you must register the named options with the option system yourself in `startup.cs`
 ```
 public class SomeController
{

    public SomeController(IOptionsUpdater<TestOptions> updater, IOptionsMonitor<TestOptions> monitor)
	{
	
	   var namedOptions = monitor.Get("Options1");
	   updater.Update((options)=>{options.SomeFlag = true; }, namedOptions, "Options1");

	   // The "foo:bar" section of the "mysettings.json" file has now been updated.
	}

}

```

## Notes

In `startup.cs` - when you call `services.ConfigureJsonUpdatableOptions<TOptions>()` the arguments you provide help specify the location within the JSON file

you to bind your options class - in this case `TestOptions` to the relevent section of the JSON file.
There are overloads so that you can pass in a `ConfigurationSection` directly if you already have the section handy.
`foo:bar` is the `sectionPath` for the location within the JSON file what will be updated when you save changes to the options.
If you don't specify a `sectionPath` then the root element in the JSON file will be overwritten - if you are using only one file per options class then this is ok,
otherwise you will need to seperate them into sections and use sectionPath's per options class.'

You need to provide factory methods responsible for returning the `Stream`s used for reading and writing the json file.
It is necessary to read the JSON file prior to updating it, because its contents must be preserved - only the section relevent to the options class will be updated. To do this, it must read through the JSON file stream, building up the JSON structure in memory, until it finds the section that needs to be updated. If the section is missing it will be added.


