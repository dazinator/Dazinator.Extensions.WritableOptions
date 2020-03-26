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

Note: There are also overloads to allow you pass in your own `JsonSerializerOptions` so you can control how stuff is written to the file - e.g things like indentation, and null value handling etc.

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


Note that when you call any of the `services.ConfigureJsonUpdatableOptions<TOptions>()` or any of it's overloads,
 it will internally also call `services.Configure<TOptions>(configuration);` on the `IConfiguration`, or `IConfigurationSection` that you provide. 
 This means your Options class is set up with the options system, so you can also use it normally in cases where you don't need to update it, by injecting the normal `IOptionsSnapshot<TOptions>' etc.

 If you want to do your own Options registration - i.e using `.AddOptions` or calling `Configure<TOptions>()` yourself, then don't use `ConfigureJsonUpdatableOptions` use `AddJsonUpdatableOptions` as that method will only add services needed for updating options and it won't also try to configure the TOptions for you with the options / configuration system - it will assume you have already done that.

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

If you don't specify a `sectionPath` (or IConfigurationSection) then the root element in the JSON file can be overwritten by an update - if you are using only one file per options class then this is ok,
otherwise if you are updating multiple options against the same file, you will need to seperate them into sections and use sectionPath's per options class.'

Be careful not to do simultaneous updates. Updates will read and write to a file - if two or more threads try to update the same file at the same time then you may get issues (I haven't tested exactly what happens but assume the usual type of errors).

An update will actually read the JSON file into a memory stream prior to updating it in memory, and then writing the fully modified stream back to the file overwriting its previous contents.
If the section corresponding to the options class is missing it will be added.