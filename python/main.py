def loadEnv():
    import os
    import sys
    import json
    import logging
    from pathlib import Path

    # Load environment variables from .env file
    from dotenv import load_dotenv
    load_dotenv()

    # Set logging level
    logging.basicConfig(level=os.getenv("LOGGING_LEVEL"))

    # Add project root to path
    sys.path.append(str(Path(__file__).parent.parent))

    # Load config file
    with open(os.getenv("CONFIG_FILE")) as f:
        config = json.load(f)

    # Set environment variables
    os.environ["CONFIG"] = json.dumps(config)

def main():
    loadEnv()
    pass

if __name__ == '__main__':
    main()