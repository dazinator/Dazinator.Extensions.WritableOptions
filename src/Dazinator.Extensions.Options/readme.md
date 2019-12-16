See comment on https://github.com/aspnet/Extensions/issues/2777

`OptionsManager` uses it's own private cache for options instances. 
`OptionsMonitor` uses a seperate singleton cache.

This library allows you to replace the default
'OptionsManager' with one that uses the same singleton cache as `OptionsMonitor` so that
they work in harmony together in mixed scenarios.
