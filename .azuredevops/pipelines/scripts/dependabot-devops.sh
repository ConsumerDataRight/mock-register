#!/bin/bash
SYSTEM_COLLECTIONURI_TRIM=`echo "${SYSTEM_COLLECTIONURI:22}"`
PROJECT_PATH="$SYSTEM_COLLECTIONURI_TRIM$SYSTEM_TEAMPROJECT/_git/$BUILD_REPOSITORY_NAME"
URLENCODED_PATH=`echo "$PROJECT_PATH"|sed 's/ /%20/g'`
echo "org: $SYSTEM_COLLECTIONURI_TRIM"
echo "project: $SYSTEM_TEAMPROJECT"
echo "repo: $BUILD_REPOSITORY_NAME"
echo "path: $PROJECT_PATH"
echo "url encoded path: $URLENCODED_PATH"
echo "package manager: $PACKAGE_MANAGER"
echo "source code path: $SOURCE_CODE_PATH"
#
echo "---[ Starting dependabot run: $SOURCE_CODE_PATH ]---"
echo `docker run  -v "$(pwd)/dependabot-script:/home/dependabot/dependabot-script" -w '/home/dependabot/dependabot-script' -e AZURE_ACCESS_TOKEN="$PERSONAL_ACCESS_TOKEN" -e PACKAGE_MANAGER="$PACKAGE_MANAGER" -e PROJECT_PATH="$URLENCODED_PATH" -e DIRECTORY_PATH="$SOURCE_CODE_PATH" dependabot/dependabot-core bundle exec ruby ./generic-update-script.rb`
echo "---[ Finished dependabot run ]---"