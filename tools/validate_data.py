#!/usr/bin/env python3
"""Validate data/** creatures for VS0+ content pipeline."""

from __future__ import annotations

import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CREATURES_DIR = ROOT / "data" / "creatures"
REQUIRED = ("id", "name_key", "aspect_primary", "regions")


def fail(msg: str) -> None:
    print(f"ERROR: {msg}", file=sys.stderr)


def main() -> int:
    if not CREATURES_DIR.is_dir():
        fail(f"missing directory: {CREATURES_DIR}")
        return 1

    files = sorted(CREATURES_DIR.glob("*.json"))
    if not files:
        fail("no creature json files found")
        return 1

    ids: set[str] = set()
    errors = 0

    for path in files:
        try:
            data = json.loads(path.read_text(encoding="utf-8"))
        except json.JSONDecodeError as exc:
            fail(f"{path.name}: invalid json ({exc})")
            errors += 1
            continue

        if not isinstance(data, dict):
            fail(f"{path.name}: root must be object")
            errors += 1
            continue

        for key in REQUIRED:
            if key not in data:
                fail(f"{path.name}: missing required field '{key}'")
                errors += 1

        creature_id = data.get("id")
        if isinstance(creature_id, str):
            if creature_id in ids:
                fail(f"{path.name}: duplicate id '{creature_id}'")
                errors += 1
            ids.add(creature_id)
            if not (len(creature_id) == 4 and creature_id[0] == "C" and creature_id[1:].isdigit()):
                fail(f"{path.name}: id '{creature_id}' should match C###")
                errors += 1
        else:
            fail(f"{path.name}: id must be string")
            errors += 1

        regions = data.get("regions")
        if not isinstance(regions, list) or len(regions) == 0:
            fail(f"{path.name}: regions must be non-empty array")
            errors += 1

        name_key = data.get("name_key")
        if not isinstance(name_key, str) or not name_key:
            fail(f"{path.name}: name_key must be non-empty string")
            errors += 1

        aspect = data.get("aspect_primary")
        if not isinstance(aspect, str) or not aspect:
            fail(f"{path.name}: aspect_primary must be non-empty string")
            errors += 1

    if errors:
        print(f"validate_data: FAILED ({errors} error(s), {len(files)} file(s))")
        return 1

    print(f"validate_data: OK ({len(files)} creature file(s), {len(ids)} id(s))")
    return 0


if __name__ == "__main__":
    sys.exit(main())
