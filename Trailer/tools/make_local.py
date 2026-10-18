"""Wraps trailer.html (artifact-style page body) into index.html for playing straight from disk.

    python Trailer/tools/make_local.py
"""
import io
import os

here = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..")
body = io.open(os.path.join(here, "trailer.html"), encoding="utf-8").read()
page = ("<!doctype html>\n<html lang=\"en\">\n<head>\n<meta charset=\"utf-8\">\n"
        "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1, viewport-fit=cover\">\n"
        "<style>html,body{margin:0}[hidden]{display:none!important}</style>\n</head>\n<body>\n"
        + body + "\n</body>\n</html>\n")
io.open(os.path.join(here, "index.html"), "w", encoding="utf-8").write(page)
print("wrote Trailer/index.html")
