use std::fs::File;
use std::env;
use std::io::{self, BufReader, Write};
use colored::*;
use std::vec::Vec;
use std::process;

struct Pair {
    replace: String,
    replace_with: String,
}

fn help() {
    let mayor_version:u32 = 1;
    let minor_version:u32 = 0;
    let revision:u32 = 0;

    println!("{}", "Replace the ocurrences of string pairs within the given file,".bright_cyan());
    println!("{}", "string1 is replaced with replacement1, string2 is replaced with replacement2 and so on.".bright_cyan());
    println!("");
    println!("{}", "Usage:".bright_white());
    println!("\t{} {} {} {} {} {} {} {} {} {} ...{}*", "replace_dealer".bright_green(), 
        "-f".bright_black(), "input_file".bright_white(), 
        "-o".bright_black(), "output_file".bright_white(), 
        "string1".bright_white(), "replacement1".bright_white(), "[".bright_black(), "string2".bright_white(), "replacement2".bright_white(), "]".bright_black());
    println!("\t{} {}", "replace_dealer".bright_green(), "-h".bright_black());

    println!("{}", "Options:".bright_cyan());
    println!("\t{}\t{}", "-i | --inputfile".bright_cyan(), "inputfile to open in read mode and make replacements (input_file is not changed).".bright_cyan());
    println!("\t{}\t{}", "-o | --outputfile".bright_cyan(), "outputfile to create with the changes.".bright_cyan());
    println!("\t{}\t\t{}", "-h | --help".bright_cyan(), "Show this help.".bright_cyan());
    println!("{} {}{}", "Third".yellow(), "3".truecolor(200, 200, 0), "ye Software Inc. © 2024".yellow());
    println!("Version: {}.{}.{}.", mayor_version, minor_version, revision);
}

fn error(msg: String, err: String) {
    println!("{} {} [{}]", "Error:".bright_red(), msg.red(), err.white());
    process::exit(0x0100);
}

fn replace_case_insensitive(text: &str, from: &str, to: &str) -> String {
    let mut result = String::new();
    let mut last_end = 0;
    while let Some(start) = text[last_end..].to_lowercase().find(&from.to_lowercase()) {
        result.push_str(&text[last_end..last_end + start]);
        result.push_str(to);
        last_end += start + from.len();
    }
    result.push_str(&text[last_end..]);
    result
}

fn read_file(infile: &str) -> Result<String, std::io::Error> {
    let file: File = File::open(infile)?;     // Input file 
    let reader: BufReader<File> = BufReader::new(file);
    let lines = utf16_reader::read_to_string(reader);
    Ok(lines)
}

fn replace_strings(lines: String, outfile: String, pairs: &Vec<Pair>) {
    let mut total: u32 = 0;
    let mut total_changed: u32 = 0;
    
    let mut output = File::create(outfile).unwrap();    // Output file

    for line in lines.lines() {
        let mut new_line: String = String::from(line);

        for pair in pairs {
            let old_line: String = new_line.clone();
            new_line = replace_case_insensitive(&new_line, &pair.replace, &pair.replace_with);
            if old_line != new_line {
                total_changed += 1;
            }
        }

        write!(output, "{}\r\n", new_line).unwrap();
        total += 1;
    }

    println!("{} {} {} {} {}",
        "Processed a total of".cyan(),
        total.to_string().yellow(), 
        "lines and made a change in".cyan(),
        total_changed.to_string().yellow(), 
        "of them.".cyan());
    println!("{}", "Finished!".green());

}

fn process_file(infile: String, outfile: String, pairs: &Vec<Pair>) -> io::Result<()> {
    let lines = read_file(infile.as_str());
    match lines {
        Ok(contents) => replace_strings(contents, outfile, pairs),
        Err(e) => error(String::from("Error opening file"), e.to_string()),
    }
    Ok(())
}

fn main() -> io::Result<()> {
    let mut proceed: bool = false;
    let mut got_infile = false;
    let mut got_outfile = false;
    let mut error: String = String::from("No error specified");

    let mut infile: String = String::from("");
    let mut outfile: String = String::from("");
    let mut pairs: Vec<Pair> = Vec::new();


    let args: Vec<String> = env::args().collect();
    // We need at least 7 args, 'cmd' '-f' 'f' '-o' 'o' 's1' 'r1'
    if args.len() == 2 || (args.len() >= 7 && (1 - ((args.len() & 1) << 1) as i32 == -1)) {
        let mut replace: String;
        let mut replace_with: String = String::from("");

        let mut args = env::args().skip(1);
        while let Some(arg) = args.next() {
            match &arg[..] {
                "-h" | "--help" => {
                    help();
                    process::exit(0x0100);
                },
                "-i" | "--infile" => {
                    if let Some(arg_infile) = args.next() {
                        infile = arg_infile;
                        got_infile = true;
                    } else {
                        error = String::from("No value specified for parameter -f | --filename.");
                    }
                }
                "-o" | "--outfile" => {
                    if let Some(arg_outfile) = args.next() {
                        outfile = arg_outfile;
                        got_outfile = true;
                    } else {
                        error = String::from("No value specified for parameter -o | --outputfile.");
                    }
                }
                x => {
                    replace = String::from(x);
                    if let Some(arg_replace_with) = args.next() {
                        replace_with = arg_replace_with;
                    } else {
                        error = String::from("Error in argument.");
                    }
                    pairs.push(Pair{replace: replace.clone(), replace_with: replace_with.clone()});
                    proceed = true;
                }
            }
        }
    } else {
        error = String::from("Error in number of arguments, check help.");
    }

    if proceed && got_infile && got_outfile {
        println!("{}\t\t'{}'", "infile:".green(), infile.yellow());
        println!("{}\t'{}'", "outfile:".green(), outfile.yellow());
        println!("{}", "replacements:".green());
        for pair in &pairs {
            println!("\t\treplace '{}'\twith '{}'", pair.replace.cyan(), pair.replace_with.green());
        }

        let _ = process_file(infile, outfile, &pairs);

    } else {
        if !got_outfile && !got_infile {
            error = String::from("No input or output file specified, please use -i or --infile and -o or --outfile");
        } else {
            if !got_infile {
                error = String::from("No input file specified, please use -i or --infile");
            }
            if !got_outfile {
                error = String::from("No output file specified, please use -o or --outfile");
            }                
        }

        println!("{} '{}'", "Error:".cyan(), error.red());
        println!("");
        help();
    }
      
    Ok(())
}
