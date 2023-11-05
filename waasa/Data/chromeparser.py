from download_file_types_pb2 import DownloadFileType, DownloadFileTypeConfig
from google.protobuf import text_format
from enum import Enum
import yaml


# https://raw.githubusercontent.com/chromium/chromium/main/components/safe_browsing/content/resources/download_file_types.asciipb
# https://raw.githubusercontent.com/chromium/chromium/main/components/safe_browsing/content/common/proto/download_file_types.proto
# https://grpc.io/docs/protoc-installation/
#  /home/dobin/tools/bin/protoc -I=. --python_out=. download_file_types.proto


class ChromeDangerLevel(Enum):
    NOT_DANGEROUS = 0
    ALLOW_ON_USER_GESTURE = 1
    DANGEROUS = 2

class ChromePlatformType(Enum):
    PLATFORM_TYPE_ANY = 0
    PLATFORM_TYPE_ANDROID = 1
    PLATFORM_TYPE_CHROME_OS = 2
    PLATFORM_TYPE_LINUX = 3
    PLATFORM_TYPE_MAC = 4
    PLATFORM_TYPE_WINDOWS = 5
    PLATFORM_TYPE_FUCHSIA = 6
    PLATFORM_TYPE_UNKNOWN = 7  # added

class AutoOpenHint(Enum):
    DISALLOW_AUTO_OPEN = 0
    ALLOW_AUTO_OPEN = 1
    UNKNOWN = 2  # added

class ChromeType():
    def __init__(self, extension, platform, danger_level, auto_open_hint):
        self.extension = extension
        self.platform = platform
        self.danger_level = danger_level
        self.auto_open_hint = auto_open_hint


def readChromeFileTypes(filename = 'download_file_types.asciipb'):
    asciipb_data = open(filename, 'r').read()
    file_types_instance = DownloadFileTypeConfig()
    text_format.Merge(asciipb_data, file_types_instance)

    chromeFiles = {}
    for file_type in file_types_instance.file_types:
        myExtension = "." + file_type.extension  # we convert the extension to include the dot
        platform = ChromePlatformType.PLATFORM_TYPE_UNKNOWN
        danger_level = ChromeDangerLevel.NOT_DANGEROUS  # assumption
        auto_open_hint = AutoOpenHint.UNKNOWN

        if file_type.platform_settings is not None and len(file_type.platform_settings) > 0:
            platform = ChromePlatformType(file_type.platform_settings[0].platform)
            danger_level = ChromeDangerLevel(file_type.platform_settings[0].danger_level)
            auto_open_hint = AutoOpenHint(file_type.platform_settings[0].auto_open_hint)

        chromeFiles[myExtension] = ChromeType(myExtension, platform, danger_level, auto_open_hint)

    return chromeFiles


def update(chromeExtensions, filename="info.yaml", write=True):
    # read our info.yaml
    with open(filename) as yamlfile:
        try:
            data = yaml.safe_load(yamlfile)
        except yaml.YAMLError as e:
            print('Decoding {} as failed with: {}'.format(filename, e))
            quit()

    for c_ext, c_type in chromeExtensions.items():
        first_or_default = next((x for x in data["Extensions"] if x["Extension"] == c_ext), None)
        if first_or_default is None:
            # the element from chrome does not exist in our info.yaml
            # maybe we should add it manually. notify user.

            # dangerous
            if c_type.danger_level.value > ChromeDangerLevel.NOT_DANGEROUS.value:
                # windows
                if (c_type.platform is ChromePlatformType.PLATFORM_TYPE_ANY or c_type.platform is ChromePlatformType.PLATFORM_TYPE_WINDOWS):
                    print("Not found: {}: {}  {}  {}".format(
                        c_ext, c_type.danger_level, c_type.auto_open_hint, c_type.platform))
        else:
            # augment the info.yaml with chrome data
            first_or_default["ChromePlatform"] = c_type.platform.name
            first_or_default["ChromeDangerLevel"] = c_type.danger_level.name
            first_or_default["ChromeAutoOpenHint"] = c_type.auto_open_hint.name

    # write it again
    if write:
        with open(filename + ".2", 'w') as yamlfile:
            yaml.dump(data, yamlfile, sort_keys=False, width=1024)

    if False:
        for fe in data["Extensions"]:
            if fe["Extension"] in chromeExtensions:
                chromeExtension = chromeExtensions[fe["Extension"]]
                fe["Platform"] = chromeExtension.platform.name
                fe["DangerLevel"] = chromeExtension.danger_level.name
                fe["AutoOpenHint"] = chromeExtension.auto_open_hint.name
            else:
                print("Extension not found: " + fe["Extension"])


def main():
    chromeExtensions = readChromeFileTypes()
    print("Chrome Extensions Count: "  + str(len(chromeExtensions)))
    update(chromeExtensions)


if __name__ == "__main__":
    main()
