# Infotrack Tech test

This is a dotnet project that utilizes built in libraries like HtmlAgilityPack to call, parse, and scrape HTML pages.

The only search engine currently available is Google. (Ran out of time, other obligations to fulfill)

Technologies used:
  - HTMLAgilityPack (build in library for calling & parsing websites, not external)
  - XUnit (Test suite)
  
## Installation

Use your favourite method to install the dotnet sdk on your machine.

e.g. 
```bash
brew install dotnet-sdk
```

Before performing any work, I recommend installing a pre-commit hook with this script.
```bash
curl --silent  https://raw.githubusercontent.com/thoughtworks/talisman/master/global_install_scripts/install.bash > /tmp/install_talisman.bash && /bin/bash /tmp/install_talisman.bash
```

## Usage

```bash
dotnet run

or

dotnet build
dotnet test
dotnet run
```

## Contributing
n/a

## License
[MIT](https://choosealicense.com/licenses/mit/)
