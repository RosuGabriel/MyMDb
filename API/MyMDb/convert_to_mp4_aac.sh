#!/bin/bash
set +x

if [ -z "$1" ]; then
    echo "Usage: $0 input_file"
    exit 1
fi

input_file="$1"

if [ ! -f "$input_file" ]; then
    echo "File '$input_file' does not exist."
    exit 1
fi

input_dir=$(dirname "$input_file")
input_name=$(basename "$input_file" | cut -d. -f1)

output_file="${input_dir}/${input_name}_temp.mp4"

ffmpeg -i "$input_file" -c:v copy -c:a aac -b:a 192k "$output_file"
if [ $? -ne 0 ]; then
    echo "Error converting the file."
    exit 1
fi

echo "File converted successfully: $output_file"

rm -f "$input_file"
if [ $? -ne 0 ]; then
    echo "Error deleting the original file. It might be in use or you might not have permission."
    exit 1
fi

mv -f "$output_file" "${input_dir}/${input_name}.mp4"
if [ $? -ne 0 ]; then
    echo "Error renaming the output file. The file might be in use or you might not have permission."
    exit 1
fi

echo "File replaced successfully: $input_file"
