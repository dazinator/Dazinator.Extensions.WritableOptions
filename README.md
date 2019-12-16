## Dazinator Extensions Options

This repo produces the following nuget packages:

- Dazinator.Extensions.Options (Support library containing optional enhancements for use with `Microsoft.Extensions.Options`)
- Dazinator.Extensions.Options.Updatable (Allows you to save changes to options instances to a JSON file at runtime.)


## Dazinator.Extensions.Options

This library provides optional enhancements for usage with `Microsoft.Extensions.Options`.

### An alternative `OptionsManager` that uses same cache as OptionsMonitor

The default `IOptionsManager` provided by Microsoft, has it's own privately initialised cache for options instances. 
`IOptionsMonitor` uses a seperate cache, registered as a singleton on startup.
This can cause problems in some scenarios where you are using both `IOptionsSnapshot` and `IOptionsMonitor` in the same request because they each will resolve your options instance from seperate caches, which can casue inconsistencies.
To overcome this, you can register a replacement `IOptionsManager` that will share the same cache.
Usage:

```
 services.AddOptions()
         .AddOptionsManagerBackedByMonitorCache // solves it..

```

This replaces the registration for `OptionsManager` with one that will share the same cache with `OptionsMonitor`.

### Dazinator.Extensions.Options.Updatable

Allows you to write an options instance with changed values to a JSON file (also allowing you to specify a section path within the JSON)

First, include the target JSON file as part of your app configuration:

`Program.cs`

```

   webBuilder.ConfigureAppConfiguration((b, c) =>
   {
       c.AddJsonFile("mysettings.json", true, true);
	  
```

Then, `AddOptions()` and then Configure your `TOptions` to be updatable.

`Startup.cs`:

```

     services.AddOptions().AddOptionsManagerBackedByMonitorCache();

	 services.ConfigureJsonUpdatableOptions<TestOptions>("foo:bar", () => File.OpenRead("mysettings.json"), () => File.OpenWrite("mysettings.json"), leaveOpen: false);
	

```

The overload used above lets you specify your own delegate for providing the Read and Write Streams for reading and writing the JSON file.
However if you just want to use System.IO you can use:

```
     services.ConfigureJsonUpdatableOptions<TestOptions>("foo:bar", new FileJsonStreamProvider<TestOptions>("C:/SettingsFolder", "/mysettings.json"));
	 
```

You can now update options by injecting `IOptionsUpdater<TestOptions>` like so:


```
public class SomeController
{

    public SomeController(IOptionsUpdater<TestOptions> updater)
	{
	
	   updater.Update((options)=>{options.SomeFlag = true; });

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

    public SomeController(IOptionsUpdater<TestOptions> updater)
	{	
	   updater.Update((options)=>{options.SomeFlag = true; }, "Options1");
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


