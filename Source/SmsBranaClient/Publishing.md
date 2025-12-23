# Publishing to NuGet

## First-Time Setup

```sh
dotnet nuget add source https://api.nuget.org/v3/index.json \
  --name nuget.org \
  --username unused \
  --password YOUR_API_KEY \
  --store-password-in-clear-text
```

On macOS/Linux this is stored in `~/.nuget/NuGet/NuGet.Config`

## Publishing a New Version

Run these commands carefully one by one:

```sh
# Increment before publishing
VERSION='1.0.0'

# Ensure clean state (stop here if it isn't)
git status

# Clean outputs
dotnet clean -c Release
rm -rf artifacts
mkdir artifacts

# Tag release
git tag -a "v${VERSION}" -m "Release v${VERSION}"
git push origin "v${VERSION}"

# Pack & publish
dotnet pack -c Release -p:Version=${VERSION} -o artifacts
dotnet nuget push artifacts/*.nupkg --source nuget.org --skip-duplicate
```