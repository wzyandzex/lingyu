#!/usr/bin/env python3
"""Validate data/** creatures, encounters, skills, enemies."""

from __future__ import annotations

import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CREATURES_DIR = ROOT / "data" / "creatures"
ENCOUNTERS_DIR = ROOT / "data" / "encounters"
SKILLS_DIR = ROOT / "data" / "skills"
ENEMIES_DIR = ROOT / "data" / "enemies"
REQUIRED = ("id", "name_key", "aspect_primary", "regions")


def fail(msg: str) -> None:
    print(f"ERROR: {msg}", file=sys.stderr)


def load_json_dir(directory: Path):
    if not directory.is_dir():
        return []
    out = []
    for path in sorted(directory.glob("*.json")):
        try:
            out.append((path, json.loads(path.read_text(encoding="utf-8"))))
        except json.JSONDecodeError as exc:
            fail(f"{path.name}: invalid json ({exc})")
    return out


def main() -> int:
    errors = 0
    creature_ids: set[str] = set()
    skill_ids: set[str] = set()

    files = load_json_dir(CREATURES_DIR)
    if not files:
        fail("no creature json files")
        return 1

    for path, data in files:
        if not isinstance(data, dict):
            fail(f"{path.name}: root must be object")
            errors += 1
            continue
        for key in REQUIRED:
            if key not in data:
                fail(f"{path.name}: missing {key}")
                errors += 1
        cid = data.get("id")
        if isinstance(cid, str):
            creature_ids.add(cid)
        skills = data.get("skills") or []
        if path.name.startswith("C001") and (not isinstance(skills, list) or len(skills) == 0):
            fail("C001 skills must be non-empty for VS3")
            errors += 1

    for path, data in load_json_dir(SKILLS_DIR):
        if isinstance(data, dict) and isinstance(data.get("id"), str):
            skill_ids.add(data["id"])
        else:
            fail(f"{path.name}: skill needs id")
            errors += 1

    if "sk_mist_veil" not in skill_ids or "sk_vine_whisper" not in skill_ids:
        fail("missing teaching skills sk_mist_veil / sk_vine_whisper")
        errors += 1

    enemy_ids: set[str] = set()
    for path, data in load_json_dir(ENEMIES_DIR):
        if isinstance(data, dict) and isinstance(data.get("id"), str):
            enemy_ids.add(data["id"])
            for sid in data.get("skills") or []:
                if sid not in skill_ids:
                    fail(f"{path.name}: unknown skill {sid}")
                    errors += 1
        else:
            fail(f"{path.name}: enemy needs id")
            errors += 1

    if "E001" not in enemy_ids:
        fail("E001 enemy required for VS3")
        errors += 1

    # C001 skill refs
    for path, data in files:
        if data.get("id") == "C001":
            for sid in data.get("skills") or []:
                if sid not in skill_ids:
                    fail(f"C001 references missing skill {sid}")
                    errors += 1

    if ENCOUNTERS_DIR.is_dir():
        for path, data in load_json_dir(ENCOUNTERS_DIR):
            for e in data.get("entries") or []:
                if isinstance(e, dict):
                    did = e.get("def_id")
                    if did and did not in creature_ids:
                        fail(f"{path.name}: def_id {did} not in creatures")
                        errors += 1

    if "C002" not in creature_ids:
        fail("C002 required")
        errors += 1

    if errors:
        print(f"validate_data: FAILED ({errors} error(s))")
        return 1
    print(
        f"validate_data: OK (creatures={len(creature_ids)}, skills={len(skill_ids)}, enemies={len(enemy_ids)})"
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
