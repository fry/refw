using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace refw.PELoader {
    /*
    public static class Constants {
        public const ushort IMAGE_DOS_SIGNATURE = 0x54AD;
        public const uint   IMAGE_NT_SIGNATURE  = 0x00004550;
    }


    // Taken from http://www.csn.ul.ie/~caolan/publink/winresdump/winresdump/doc/pefile2.html
    [StructLayout(LayoutKind.Sequential)]
    public struct _IMAGE_DOS_HEADER {
        public ushort e_magic;         // Magic number
        public ushort e_cblp;          // Bytes on last page of file
        public ushort e_cp;            // Pages in file
        public ushort e_crlc;          // Relocations
        public ushort e_cparhdr;       // Size of header in paragraphs
        public ushort e_minalloc;      // Minimum extra paragraphs needed
        public ushort e_maxalloc;      // Maximum extra paragraphs needed
        public ushort e_ss;            // Initial (relative) SS value
        public ushort e_sp;            // Initial SP value
        public ushort e_csum;          // Checksum
        public ushort e_ip;            // Initial IP value
        public ushort e_cs;            // Initial (relative) CS value
        public ushort e_lfarlc;        // File address of relocation table
        public ushort e_ovno;          // Overlay number
        public fixed ushort e_res[4];  // Reserved words
        public ushort e_oemid;         // OEM identifier (for e_oeminfo)
        public ushort e_oeminfo;       // OEM information; e_oemid specific
        public fixed ushort e_res2[10];// Reserved words
        public long e_lfanew;          // File address of new exe header
      }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _IMAGE_NT_HEADERS {
        uint Signature;
        IMAGE_FILE_HEADER FileHeader;
        IMAGE_OPTIONAL_HEADER32 OptionalHeader;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _IMAGE_DATA_DIRECTORY {
        uint VirtualAddress;     // RVA of the data
        uint Size;               // Size of the data
    }
     * */
}
