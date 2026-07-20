#!/usr/bin/env python3
"""Validate data/** creatures and encounters for content pipeline."""

from __future__ import annotations

import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CREATURES_DIR = ROOT / "data" / "creatures"
ENCOUNTERS_DIR = ROOT / "data" / "encounters"
REQUIRED = ("id", "name_key", "aspect_primary", "regions")


def fail(msg: str) -> None:
    print(f"ERROR: {msg}", file=sys.stderr)


def main() -> int:
    errors = 0
    creature_ids: set[str] = set()

    if not CREATURES_DIR.is_dir():
        fail(f"missing directory: {CREATURES_DIR}")
        return 1

    files = sorted(CREATURES_DIR.glob("*.json"))
    if not files:
        fail("no creature json files found")
        return 1

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
            if creature_id in creature_ids:
                fail(f"{path.name}: duplicate id '{creature_id}'")
                errors += 1
            creature_ids.add(creature_id)
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

    # Encounters hard validation (VS1)
    if not ENCOUNTERS_DIR.is_dir():
        fail(f"missing directory: {ENCOUNTERS_DIR}")
        errors += 1
    else:
        enc_files = sorted(ENCOUNTERS_DIR.glob("*.json"))
        if not enc_files:
            fail("no encounter json files found")
            errors += 1
        for path in enc_files:
            try:
                data = json.loads(path.read_text(encoding="utf-8"))
            except json.JSONDecodeError as exc:
                fail(f"{path.name}: invalid json ({exc})")
                errors += 1
                continue
            if not isinstance(data, dict) or "id" not in data:
                fail(f"{path.name}: must be object with id")
                errors += 1
                continue
            entries = data.get("entries")
            if not isinstance(entries, list) or not entries:
                fail(f"{path.name}: entries must be non-empty array")
                errors += 1
                continue
            for i, e in enumerate(entries):
                if not isinstance(e, dict):
                    fail(f"{path.name}: entries[{i}] must be object")
                    errors += 1
                    continue
                def_id = e.get("def_id")
                if not isinstance(def_id, str) or not def_id:
                    fail(f"{path.name}: entries[{i}].def_id required")
                    errors += 1
                elif def_id not in creature_ids:
                    fail(f"{path.name}: entries[{i}].def_id '{def_id}' not found in creatures")
                    errors += 1

    if "C002" not in creature_ids:
        fail("C002 required for VS1")
        errors += 1

    if errors:
        print(f"validate_data: FAILED ({errors} error(s))")
        return 1

    print(f"validate_data: OK ({len(files)} creature(s), encounters checked, {len(creature_ids)} id(s))")
    return 0


if __name__ == "__main__":
    sys.exit(main())
