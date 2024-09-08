import fs from 'node:fs'
import path from 'node:path'
import { execSync } from "node:child_process"

// Target duration is set to Twitch's default which is 10 seconds
const playlistDefaults = ["#EXTM3U", "#EXT-X-VERSION:3", "#EXT-X-TARGETDURATION:10", "#EXT-X-PLAYLIST-TYPE:EVENT", "#EXT-X-MEDIA-SEQUENCE:0"]
// Example usage: node .\index.mjs "D:\\11-04-2023\\"
const playlistPath = process.argv[2]
const playlistWriteStream = fs.createWriteStream(path.join(playlistPath, 'playlist.m3u8'))

fs.readdir(path.join(playlistPath), async (err, files) => {
    // This only works for file names like 0.ts-1.ts... (also twitch default)
    files.sort((a, b) => {
        return parseInt(a.split('.')[0]) - parseInt(b.split('.')[0])
    })
    playlistDefaults.forEach((line) => {
        playlistWriteStream.write(`${line}\n`)
    })
    files.forEach(async (file, i) => {
        if (file === 'playlist.m3u8') return
        var stdout = execSync(`ffprobe -i ${path.join(playlistPath, file)} -show_entries format=duration -v quiet -of csv="p=0"`)
        playlistWriteStream.write(`#EXTINF:${(parseFloat(stdout.toString().replace('\n', ''))).toFixed(3)},\n${file}\n`)
    })
    playlistWriteStream.write("#EXT-X-ENDLIST")
    playlistWriteStream.end()
})