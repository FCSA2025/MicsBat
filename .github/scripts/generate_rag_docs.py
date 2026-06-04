import os
import re
import json
from pathlib import Path

# Setup output directory for Bedrock files
OUTPUT_DIR = Path("./rag_output")
OUTPUT_DIR.mkdir(exist_ok=True)

def process_csharp_file(file_path):
    try:
        content = file_path.read_text(encoding="utf-8", errors="ignore")
        
        # Automatically find the namespace using regex
        namespace_match = re.search(r"namespace\s+([\w\.]+)", content)
        namespace = namespace_match.group(1) if namespace_match else "Global"
        
        # Clean folder calculations for Windows systems
        relative_path = file_path.relative_to(Path.cwd())
        path_parts = list(relative_path.parent.parts)
        
        # Ensure metadata values are simple strings for Bedrock compatibility
        folder_layer = path_parts[0] if path_parts else "Root"
        sub_layer = path_parts[1] if len(path_parts) > 1 else "None"

        # 1. Structure the Markdown Content for the AI
        md = f"# Documented File: {file_path.name}\n"
        md += f"**Repository Path:** `{relative_path}`\n"
        md += f"**Primary Layer:** `{folder_layer}`\n"
        md += f"**Namespace:** `{namespace}`\n\n"
        md += "## Source Code Representation\n"
        md += f"```csharp\n{content}\n```\n"
        
        # Create a safe, unique filename using double underscores for slashes
        safe_name = str(relative_path).replace(os.sep, "__").replace(".cs", ".md")
        
        # Write the Markdown text file
        md_file = OUTPUT_DIR / safe_name
        md_file.write_text(md, encoding="utf-8")
        
        # 2. Build the Mandatory Bedrock Metadata Sidecar File
        metadata = {
            "metadataAttributes": {
                "file_name": file_path.name,
                "repo_path": str(relative_path),
                "folder_layer": folder_layer,
                "sub_layer": sub_layer,
                "namespace": namespace,
                "language": "CSharp"
            }
        }
        
        # Write the metadata JSON file
        metadata_file = OUTPUT_DIR / f"{safe_name}.metadata.json"
        metadata_file.write_text(json.dumps(metadata, indent=2), encoding="utf-8")
        
    except Exception as e:
        print(f"Skipping file {file_path} due to error: {e}")

# Main execution: Recursively crawl the repository
print("Starting repository crawl...")
file_count = 0

for cs_file in Path.cwd().glob("**/*.cs"):
    # Ignore compiled, build, and system folders explicitly
    ignored_folders = ["bin", "obj", ".vs", "packages", ".github", "rag_output"]
    if any(folder in cs_file.parts for folder in ignored_folders):
        continue
        
    process_csharp_file(cs_file)
    file_count += 1

print(f"Crawl finished successfully! Formatted {file_count} files for Bedrock.")
