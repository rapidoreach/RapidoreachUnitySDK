#!/usr/bin/env python3
"""Build the versioned Unity package without requiring an activated editor."""

from __future__ import annotations

import hashlib
import io
import pathlib
import tarfile


ROOT = pathlib.Path(__file__).resolve().parents[1]
ASSET_ROOT = ROOT / "Assets" / "RapidoReach"
OUTPUT = ROOT / "RapidoreachUnitySDK-2.0.0.unitypackage"


def guid(path: str) -> str:
    return hashlib.sha256(f"rapidreach-unity-v2:{path}".encode()).hexdigest()[:32]


def meta(path: pathlib.Path, asset_path: str) -> bytes:
    value = guid(asset_path)
    if path.is_dir():
        body = f"fileFormatVersion: 2\nguid: {value}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n"
    elif path.suffix == ".cs":
        body = f"fileFormatVersion: 2\nguid: {value}\nMonoImporter:\n  externalObjects: {{}}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {{instanceID: 0}}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n"
    elif path.suffix == ".asmdef":
        body = f"fileFormatVersion: 2\nguid: {value}\nAssemblyDefinitionImporter:\n  externalObjects: {{}}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n"
    else:
        body = f"fileFormatVersion: 2\nguid: {value}\nDefaultImporter:\n  externalObjects: {{}}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n"
    return body.encode()


def add_bytes(archive: tarfile.TarFile, name: str, payload: bytes) -> None:
    info = tarfile.TarInfo(name)
    info.size = len(payload)
    info.mode = 0o644
    info.mtime = 0
    archive.addfile(info, io.BytesIO(payload))


def main() -> None:
    entries = [ASSET_ROOT]
    entries.extend(sorted(ASSET_ROOT.rglob("*"), key=lambda value: value.as_posix()))
    with tarfile.open(OUTPUT, "w:gz", format=tarfile.PAX_FORMAT) as archive:
        for path in entries:
            relative = path.relative_to(ROOT).as_posix()
            package_id = guid(relative)
            add_bytes(archive, f"{package_id}/pathname", relative.encode())
            add_bytes(archive, f"{package_id}/asset.meta", meta(path, relative))
            if path.is_file():
                add_bytes(archive, f"{package_id}/asset", path.read_bytes())
    print(f"built {OUTPUT} ({OUTPUT.stat().st_size} bytes, {len(entries)} entries)")


if __name__ == "__main__":
    main()
