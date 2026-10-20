#!/usr/bin/env bash
# Builds the mod DLL and packages dist/peatanddeadwood_<version>.zip.
# Pass --deploy to also copy the mod into the game's Mods folder.
set -euo pipefail
cd "$(dirname "$0")"

DOTNET="${DOTNET:-/mnt/c/Program Files/dotnet/dotnet.exe}"
MODS="${MODS:-/mnt/c/Users/kamro/AppData/Roaming/VintagestoryData/Mods}"
VERSION=$(python3 -c 'import json; print(json.load(open("modinfo.json"))["version"])')

rm -rf build
(cd src && "$DOTNET" build -c Release -o ../build -nologo -v quiet)

mkdir -p dist
ZIP="dist/peatanddeadwood_${VERSION}.zip"
rm -f "$ZIP"
python3 - "$ZIP" <<'PY'
import os, sys, zipfile
with zipfile.ZipFile(sys.argv[1], "w", zipfile.ZIP_DEFLATED) as z:
    for path in ("modinfo.json", "LICENSE"):
        z.write(path)
    for root, _, files in os.walk("assets"):
        for name in sorted(files):
            z.write(os.path.join(root, name))
    z.write("build/PeatAndDeadwood.dll", "PeatAndDeadwood.dll")
PY
echo "Packaged $ZIP"

if [[ "${1:-}" == "--deploy" ]]; then
    rsync -a --delete assets modinfo.json build/PeatAndDeadwood.dll "$MODS/peatanddeadwood/"
    echo "Deployed to $MODS/peatanddeadwood/"
fi
