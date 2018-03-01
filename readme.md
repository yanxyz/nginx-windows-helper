# nginx-windows-helper

A command line tool to:

- create vhost
- start/restart/stop Nginx service

[Download](https://pan.baidu.com/s/1miqIlO8)

Notice: .Net Framework 4.7 is required,
[download](https://www.microsoft.com/net/download/Windows/run)

## Usage

1. Install [Nginx for Windows](http://nginx.org/en/docs/windows.html)
1. Register Nginx to be a Windows service by NSSM
[see here](https://stackoverflow.com/a/41467168/1405946)
1. Rename this program to a short name, e.g. ng.exe, add it to PATH
1. Create a config file along with this program with the same name(ng.ini), refer to sample.ini
1. Create the conf template, refer to sample.conf

```
ng
```

## License

MIT (c) Ivan Yan
