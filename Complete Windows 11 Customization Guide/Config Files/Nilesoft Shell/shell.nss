settings
{
	priority=1
	exclude.where = !process.is_explorer
	showdelay = 200
	// Options to allow modification of system items
	modify.remove.duplicate=1
	tip.enabled=true
}

import 'imports/theme.nss'
import 'imports/images.nss'

import 'imports/modify.nss'

menu(mode="multiple" title="Pin/Unpin" image=icon.pin)
{
}

menu(mode="multiple" title=title.more_options image=icon.more_options)
{
}

//import 'imports/terminal.nss'
import 'imports/file-manage.nss'
//import 'imports/develop.nss'
//import 'imports/goto.nss'
import 'imports/taskbar.nss'


// Author: Rubic / RubicBG
// Based on: Nilesoft Shell original snippet
// https://github.com/RubicBG/Nilesoft-Shell-Snippets/

menu(title='Shell Menu Mode' image=\uE0C5 vis=if(!sys.is11, 'disable')) {
	item(image=[\uE249, image.color2] title='Nilesoft Only' tip='Use only Nilesoft Shell context menu (removes Windows 11 "modern" context menu completely)'
		// admin cmd-ps=`& '@quote(app.exe)' '-s' '-u' '-t' '-restart'; & '@quote(app.exe)' '-s' '-r' '-t';`
		window='hidden' admin cmd-line=`/c call @quote(app.exe) -s -u -t -restart & call @quote(app.exe) -s -r -t`)
	$svg_hybrid = image.svg('<svg x="0px" y="0px" viewBox="0 0 512 512">
	  <path fill="@image.color2" d="M324.267,0c-87.951,0-161.765,60.484-182.142,142.124c14.602-3.645,29.879-5.591,45.609-5.591c103.682,0,187.733,84.052,187.733,187.733c0,15.732-1.946,31.007-5.591,45.609C451.516,349.498,512,275.685,512,187.733C512,84.052,427.948,0,324.267,0z"/>
	  <path fill="@image.color1" d="M324.267,375.467c-103.682,0-187.733-84.052-187.733-187.733c0-15.732,1.946-31.007,5.591-45.609C60.484,162.502,0,236.315,0,324.267C0,427.948,84.052,512,187.733,512c87.951,0,161.765-60.484,182.142-142.124C355.273,373.521,339.997,375.467,324.267,375.467z"/></svg>')
	item(image=svg_hybrid title='Hybrid Mode' tip='Combine Windows context menu with Nilesoft Shell via "Show more options" or Shift+Right-Click' commands {
		cmd = {	cfg_read = io.file.read(app.cfg)
				modified = regex.replace(cfg_read, '\s*(?:settings\.priority|priority)\s*=\s*(?:true|false|[01])', "\n")
				if(cfg_read!=modified, msg('Remove settings.priority from shell.nss', 'Nilesoft Shell', msg.warning) & io.delete(app.cfg) & io.file.create(app.cfg, modified)) } wait=1,
		// admin cmd-ps=`& '@quote(app.exe)' '-s' '-u' '-t' '-restart'; Sleep 2; & '@quote(app.exe)' '-s' '-r';`})
		window='hidden' admin cmd-line=`/c reg.exe delete "HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}" /f & call @quote(app.exe) -s -u -t -restart & timeout /t 3 /nobreak & call @quote(app.exe) -s -r` })
	separator()
	$svg_win = image.svg('<svg width="100" height="100" viewBox="-50 -50 612 612" >
	  <polygon style="fill:#90C300;" points="242.526,40.421 512,0 512,239.832 242.526,239.832 "/>
	  <polygon style="fill:#F8672C;" points="0,75.453 206.596,44.912 206.596,242.526 0,242.526 "/>
	  <polygon style="fill:#FFC400;" points="242.526,471.579 512,512 512,278.456 242.526,278.456 "/>
	  <polygon style="fill:#00B4F2;" points="0,436.547 206.596,467.088 206.596,278.456 0,278.456 "/></svg>')
	item(image=svg_win title='Windows Default' keys='SHIFT run' tip='Restore original Windows 11 context menu (disables Nilesoft Shell until manually re-enabled)' + "\n\n" + 'Hold SHIFT to run Nilesoft Shell afterwards.'  commands {
		cmd = {	cfg_read = io.file.read(app.cfg)
				modified = regex.replace(cfg_read, '\s*(?:settings\.priority|priority)\s*=\s*(?:true|false|[01])', "\n")
				if(cfg_read!=modified, msg('Remove settings.priority from shell.nss', 'Nilesoft Shell', msg.warning) & io.delete(app.cfg) & io.file.create(app.cfg, modified)) } wait=1, 
		// admin cmd-ps=`& '@quote(app.exe)' '-s' '-u' '-t' '-restart'; Sleep 2; & '@quote(app.exe)' '-s' '-r';`})
		window='hidden' admin cmd-line=`/c reg.exe delete "HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}" /f & call @quote(app.exe) -s -u -t -restart @if(key.shift(), '& call @quote(app.exe)')` })
}




// Author: Rubic / RubicBG
// https://github.com/RubicBG/Nilesoft-Shell-Snippets/

// remove keyboard shortcut info of the only command that has one
// modify(find='Undo*|Redo*' keys='')
// modify(where=str.start(this.title, title.Undo) or str.start(this.title, title.Redo) keys='')

//> https://support.microsoft.com/en-us/windows/keyboard-shortcuts-in-windows-dcc61a57-8ff0-cffe-9796-cb9706c75eec
//> https://www.elevenforum.com/t/keyboard-shortcuts-in-windows-11.2253/
modify(where=this.title==title.open							keys='Enter')
modify(where=this.title==title.cut				type='*'	keys='Ctrl+X')
modify(where=this.title==title.copy				type='*'	keys='Ctrl+C')
modify(where=this.title==title.paste			type='*'	keys='Ctrl+V') // tip='Open the clipboard history (Win+V)'
modify(where=this.title==title.delete			type='*'	keys='Ctrl+D' tip=["\xE1A7 Press SHIFT key to permanently delete selected file"+if(sel.count>1, 's'), tip.danger, 1.0])
modify(where=this.title==title.rename						keys='F2')
modify(where=str.start(this.title, title.undo)	type='*'	keys='Ctrl+Z')
modify(where=str.start(this.title, title.redo)	type='*'	keys='Ctrl+Y')
modify(where=this.title==title.select_all 		type='*'	keys='Ctrl+A')
modify(where=this.title==title.copy_as_path					keys='Ctrl+Shift+C')
modify(where=this.title==title.refresh						keys='Ctrl+R')
modify(where=this.title==title.properties & sel.count>0		keys='Alt+Enter')

//> https://www.elevenforum.com/t/change-size-of-desktop-icons-in-windows-11.4820/
modify(where=this.title==title.extra_large_icons	in=str.replace(title.view, '&', '') keys='Ctrl+Shift+1')
modify(where=this.title==title.large_icons			in=str.replace(title.view, '&', '') keys='Ctrl+Shift+2')
modify(where=this.title==title.medium_icons			in=str.replace(title.view, '&', '') keys='Ctrl+Shift+3')
modify(where=this.title==title.small_icons			in=str.replace(title.view, '&', '') keys='Ctrl+Shift+4')
modify(where=this.title==title.list					in=str.replace(title.view, '&', '') keys='Ctrl+Shift+5')
modify(where=this.title==title.details				in=str.replace(title.view, '&', '') keys='Ctrl+Shift+6')
modify(where=this.title==title.tiles				in=str.replace(title.view, '&', '') keys='Ctrl+Shift+7')
modify(where=this.title==title.content				in=str.replace(title.view, '&', '') keys='Ctrl+Shift+8')

// Add keyboard shortcut info to "&Folder" in "Ne&w" menu
modify(where=this.title==eval(str.res('shell32.dll', -30317)) in=str.replace(title.new, '&', '') keys='Ctrl+Shift+N')

// Add new commands for "Select &all"
item(title='Select &all' keys='Ctrl+A' image=icon.select_all cmd=command.select_all)

// Add keyboard shortcut info to "Size &All Columns to Fit"
modify(type='*' keys='Ctrl+"Num Pad +"'
	where=(this.title==eval(str.res('shell32.dll', -37201)) or this.title==eval(str.res('shell32.dll', -37466))) and (wnd.parent.name=="SHELLDLL_DefView"))

// Add new commands in "View" menu
item(title='Column Arrange' keys='Ctrl+"Num Pad +"'
	where=window.name=='CabinetWClass' menu=title.view image=icon.details sep='top'
	commands{cmd=keys.send(key.ctrl, key.shift, 54) wait=1, cmd=keys.send(key.ctrl, 107)})
item(title='Expand Navigation Pane' keys='Ctrl+Shift+E'
	where=window.name=='CabinetWClass' menu=title.view image=icon.expand sep='bottom'
	tip='Expands all folders from the tree in the navigation pane.' cmd=keys.send(key.ctrl, key.shift, 69))
item(title='Up' keys='Alt+Up'
	where=window.name=='CabinetWClass' menu=title.view image=\uE214 cmd=keys.send(key.alt, key.up))
item(title='Back' keys='Alt+Left'
	where=window.name=='CabinetWClass' menu=title.view image=\uE216 cmd=keys.send(key.alt, key.left))
item(title='Forward' keys='Alt+Right'
	where=window.name=='CabinetWClass' menu=title.view image=\uE215 cmd=keys.send(key.alt, key.right))

item(title='Desktop icon settings' where=wnd.is_desktop menu=title.view pos=indexof(quote(str.replace(title.show_desktop_icons, '&', ''))) image=image.res('@sys.bin\desk.cpl', 0)
	cmd='rundll32.exe' args='shell32.dll,Control_RunDLL desk.cpl,,0')

/* this snippet is not working well
item(title='&Hide file names' where=wnd.is_desktop menu=title.view pos=indexof(quote(str.replace(title.show_desktop_icons, '&', ''))) tip='hides file names on desctop icons temporarly'
	cmd=wnd.command(28727)) */


// Author: Rubic / RubicBG
// https://github.com/RubicBG/Nilesoft-Shell-Snippets/

// temporary replace: path.file.ext(sel[x]) with io.meta(sel[x], 'System.FileExtension')
$svg_renamer=image.svg('<svg width="100" height="100" viewBox="0 0 100 100">
  <path fill="@image.color1" d="M34 48c-12 0-19 1-21 2-4.9 2-5 12-1 14 6 5 13 3 18-2 4-5 4-9 4-14m8-3v27h-8v-7c-7 8-21 11-28.4 4C0 63 1 50 6.7 46 14 40 24 42 34 42c-1-18-19-13-29.2-9v-7C17 22 27 20 37 28c4 4 5 9 5 17"/>
  <path fill="@image.color2" d="M63 50c1 18 21 20 35 12v8c-20 7-43 3-43-22 0-16 8-25 23-25 17 0 21 11 21 27H63m29-6c-1-7-4-15-13-15-10 0-15 8-16 15h29"/>
  <path fill="@image.color1" d="m46 2-1 96h7V2z"/></svg>')
menu(title='Fast Renamer' mode='multiple' type='dir|file' image=svg_renamer) {
	item(title='Rename...' mode='single' image=icon.rename tip='Rename a single file or folder.'
		cmd=if(input('Nilesoft Shell', 'New @if(sel.type==3, 'folder', 'file') for @(sel.name)') and len(input.result)>0 ,
		io.rename(sel.path, path.combine(path.location(sel.path), input.result + sel.file.ext)), msg('no input')))
	item(title='Rename' + if(sel.count>1, ' @sel.count Selected') + '...' mode='multiple' image=icon.rename tip='Rename multiple files or folders one by one. The process can not be stopped once started.'
		cmd=if(input('Nilesoft Shell', 'New @if(sel.type==3, 'folder', 'file') name for @(sel.name)') and len(input.result)>0,
		io.rename(sel.path, path.combine(path.location(sel.path), input.result + sel.file.ext)), msg('no input')) invoke=1)
	item(title='Rename with Counter...' image=icon.rename tip='Initiate counting from the first file by pressing the SHIFT key.'
		cmd=for(x=0, x<sel.count) {
			if(x==0, if(input('Nilesoft Shell', 'New file/folder name...')==0 or len(input.result)==0, msg('no input') & break ))
			io.rename(sel[x], path.combine(path.location(sel[x]), input.result + if(x>0 or keys.shift(), ' ('+ if(keys.shift(), 1+x, x) +')') + io.meta(sel[x], 'System.FileExtension'))) })
	separator()
	item(mode='single' type='file' mode='multi_unique' title='Change Extension' image=\uE0B5
		cmd=if(cmd.invoked || (input('Nilesoft Shell', 'Type extension') and len(input.result)>0),
		io.rename(sel.path, path.join(sel.dir, sel.file.title + '.' + str.trim(input.result, "."))), msg('no input') & break ) invoke=1)
	separator()
	$svg_lcase=image.svg('<svg width="100" height="100" viewBox="0 0 16 16">
		<path fill="@image.color1" d="m7.29 11.1h-1.06v-0.7q-0.14 0.1-0.38 0.3t-0.47 0.3q-0.26 0.1-0.61 0.2-0.34 0.1-0.81 0.1-0.85 0-1.45-0.6-0.59-0.6-0.59-1.43 0-0.72 0.3-1.16 0.32-0.45 0.89-0.71 0.57-0.25 1.38-0.34t1.74-0.14v-0.16q0-0.36-0.13-0.6-0.12-0.24-0.36-0.37-0.23-0.13-0.54-0.18-0.32-0.05-0.67-0.05-0.42 0-0.93 0.12-0.52 0.11-1.06 0.31h-0.06v-1.08q0.31-0.08 0.9-0.18t1.16-0.1q0.67 0 1.16 0.11 0.5 0.11 0.86 0.37t0.54 0.68q0.19 0.41 0.19 1.02zm-1.06-1.56v-1.76q-0.49 0-1.15 0.1-0.65 0-1.04 0.16-0.46 0.12-0.74 0.4-0.28 0.27-0.28 0.75 0 0.55 0.32 0.81 0.33 0.3 1.01 0.3 0.56 0 1.02-0.2 0.47-0.25 0.86-0.56z"/>
		<path fill="@image.color2" d="m14.3 11.1h-1.1v-0.7q-0.1 0.1-0.4 0.3-0.2 0.2-0.5 0.3-0.2 0.1-0.6 0.2-0.3 0.1-0.8 0.1-0.8 0-1.43-0.6-0.59-0.6-0.59-1.43 0-0.72 0.31-1.16 0.31-0.45 0.91-0.71 0.5-0.25 1.4-0.34 0.8-0.09 1.7-0.14v-0.16q0-0.36-0.1-0.6-0.2-0.24-0.4-0.37t-0.5-0.18q-0.4-0.05-0.7-0.05-0.4 0-0.9 0.12-0.6 0.11-1.1 0.31h-0.06v-1.08q0.31-0.1 0.86-0.18 0.6-0.1 1.2-0.1 0.7 0 1.2 0.11t0.8 0.37q0.4 0.26 0.6 0.68 0.2 0.41 0.2 1.02zm-1.1-1.56v-1.76q-0.5 0-1.2 0.1-0.6 0-1 0.16-0.5 0.13-0.7 0.41-0.32 0.27-0.32 0.75 0 0.55 0.32 0.81 0.3 0.3 1 0.3 0.6 0 1-0.2 0.5-0.25 0.9-0.56z"/></svg>')
	$svg_mcase=image.svg('<svg width="100" height="100" viewBox="0 0 16 16">
		<path fill="@image.color1" d="m8.84 12h-1.2l-0.82-2.35h-3.65l-0.83 2.35h-1.13l3.07-8.43h1.49zm-2.37-3.31-1.48-4.14-1.48 4.14z"/>
		<path fill="@image.color2" d="m14.9 12h-1v-0.7q-0.2 0.1-0.4 0.3t-0.5 0.3q-0.2 0.1-0.6 0.2-0.3 0.1-0.8 0.1-0.8 0-1.4-0.6-0.63-0.6-0.63-1.4 0-0.75 0.31-1.19 0.32-0.45 0.92-0.71 0.5-0.25 1.3-0.34 0.9-0.09 1.8-0.14v-0.16q0-0.36-0.1-0.6-0.2-0.24-0.4-0.38-0.2-0.13-0.5-0.17-0.4-0-0.7-0-0.4 0-0.9 0.12-0.6 0.1-1.1 0.31h-0.1v-1.13q0.3-0.1 0.9-0.18 0.6-0.11 1.2-0.11 0.7 0 1.2 0.12 0.5 0.11 0.8 0.37 0.4 0.26 0.6 0.67 0.1 0.42 0.1 1.03zm-1-1.6v-1.72q-0.5 0-1.2 0.1-0.6 0.1-1 0.16-0.5 0.13-0.7 0.41-0.3 0.27-0.3 0.76 0 0.5 0.3 0.8t1 0.3q0.6 0 1-0.2 0.5-0.3 0.9-0.6z"/></svg>')
	$svg_ucase=image.svg('<svg width="100" height="100" viewBox="0 0 16 16">
		<path fill="@image.color1" d="m7.93 12.1h-1.19l-0.83-2.35h-3.64l-0.83 2.35h-1.138l3.068-8.43h1.49zm-2.36-3.31-1.48-4.14-1.48 4.14z"/>
		<path fill="@image.color2" d="m15.9 12.1h-1.2l-0.9-2.35h-3.6l-0.83 2.35h-1.14l3.07-8.43h1.5zm-2.4-3.31-1.5-4.14-1.5 4.14z"/></svg>')
	item(title='Lowercase' image=svg_lcase
		cmd=io.rename(sel.path, str.lower(sel.path)) invoke=1)
	item(title='Uppercase' image=svg_ucase
		cmd=io.rename(sel.path, str.upper(sel.path)) invoke=1)
	item(title='Mixed-Case' image=svg_mcase
		cmd=if(path.type(sel.path) == 3, io.rename(sel.path, str.capitalize(sel.path)),  io.rename(sel.path, str.capitalize(path.removeextension(sel.path)) + str.lower(sel.file.ext))) invoke=1)
	separator()
	item(title='Strip a Double Space'
		cmd=io.rename(sel.path, str.replace(sel, '  ', ' ')) invoke=1)
	item(title='Strip Double Spaces'
		cmd=for(x=0, x<sel.count) {
			tmp=null
			for(y=0, y>=0) /* while-infinite-loop */ {
				if(y==0, {tmp=path.name(sel[x])})
				{tmp=str.replace(tmp, "  ", " ")}
				if(str.find(tmp, '  ')>0, continue) /* while-condition */
				io.rename(sel[x], tmp)
				break /* while-break-point */ } })
	separator()
	$svg_dotsspaces=image.svg('<svg width="24" height="24" fill="none" viewBox="0 0 24 24">
		<path fill="@image.color1" d="M21 14v2c0 1.7-1.3 3-3 3H6c-1.7 0-3-1.3-3-3V14c0-.6.4-1 1-1s1 .4 1 1v2c0 .6.4 1 1 1H18c.6 0 1-.4 1-1V14c0-.6.4-1 1-1s1 .4 1 1Z" />
		<path fill="@image.color2" d="M8.75 1.5575 9.8075.5 17.75 8.4425V3.5H19.25V11h-7.5V9.5H16.6925L8.75 1.5575Z"/>
		<path fill="@image.color1" d="M5 7A2 2 90 103 5a2 2 90 002 2Zm0 2a4 4 90 114-4A4 4 90 015 9" /></svg>')
	$svg_spacesdots=image.svg('<svg width="24" height="24" fill="none" viewBox="0 0 24 24">
		<path fill="@image.color2" d="M 1.5575 7.75 L 0.5 8.8075 L 8.4425 16.75 H 3.5 V 18.25 H 11 V 10.75 H 9.5 V 15.6925 L 1.5575 7.75 Z" />
		<path fill="@image.color1" d="M 19 1 c 0 -0.6 -0.4 -1 -1 -1 s -1 0.4 -1 1 v 2 c 0 0.6 -0.4 1 -1 1 H 4 c -0.6 0 -1 -0.4 -1 -1 v -2 c 0 -0.6 -0.4 -1 -1 -1 s -1 0.4 -1 1 v 2 c 0 1.7 1.3 3 3 3 h 12 c 1.7 0 3 -1.3 3 -3 V 1 z"/>
		<path fill="@image.color1" d="M 17 11 A 4 4 0 1 0 21 15 A 4 4 0 0 0 17 11" /></svg>')
	$svg_underscoresspaces=image.svg('<svg width="24" height="24" fill="none" viewBox="0 0 24 24">
		<path fill="@image.color1" d="M21 14v2c0 1.7-1.3 3-3 3H6c-1.7 0-3-1.3-3-3V14c0-.6.4-1 1-1s1 .4 1 1v2c0 .6.4 1 1 1H18c.6 0 1-.4 1-1V14c0-.6.4-1 1-1s1 .4 1 1Z" />
		<path fill="@image.color2" d="M8.75 1.5575 9.8075.5 17.75 8.4425V3.5h1.5V11h-7.5V9.5h4.9425L8.75 1.5575Z"/>
		<path fill="@image.color1" d="M2 7h6v2H1V7z" /></svg>')
	$svg_spacesunderscores=image.svg('<svg width="24" height="24" fill="none" viewBox="0 0 24 24">
		<path fill="@image.color2" d="M 1.5575 7.75 L 0.5 8.8075 L 8.4425 16.75 H 3.5 V 18.25 H 11 V 10.75 H 9.5 V 15.6925 L 1.5575 7.75 Z" />
		<path fill="@image.color1" d="M 19 1 c 0 -0.6 -0.4 -1 -1 -1 s -1 0.4 -1 1 v 2 c 0 0.6 -0.4 1 -1 1 H 4 c -0.6 0 -1 -0.4 -1 -1 v -2 c 0 -0.6 -0.4 -1 -1 -1 s -1 0.4 -1 1 v 2 c 0 1.7 1.3 3 3 3 h 12 c 1.7 0 3 -1.3 3 -3 V 1 z"/>
		<path fill="@image.color1" d="M 13 17 h 8 v 2 h -8 v -1.875 z" /></svg>')
	item(title='Dots to Spaces' image=svg_dotsspaces
		cmd=io.rename(sel.path, path.combine(path.location(sel.path), str.replace(path.title(sel.path), '.', ' ') + sel.file.ext)) invoke=1)
	item(title='Spaces to Dots' image=svg_spacesdots
		cmd=io.rename(sel.path, path.combine(path.location(sel.path), str.replace(path.title(sel.path), ' ', '.') + sel.file.ext)) invoke=1)
	item(title='Underscores to Spaces' image=svg_underscoresspaces
		cmd=io.rename(sel.path, path.combine(path.location(sel.path), str.replace(path.title(sel.path), '_', ' ') + sel.file.ext)) invoke=1)
	item(title='Spaces to Underscores' image=svg_spacesunderscores
		cmd=io.rename(sel.path, path.combine(path.location(sel.path), str.replace(path.title(sel.path), ' ', '_') + sel.file.ext)) invoke=1)
	separator()
	item(title='Find and Replace...' keys='counter' image=\uE8AC tip='Replace text in filename (format: find|replace); Case sensitive.'
		cmd=if(input('Find and Replace', 'Enter: find_text|replace_text') and len(input.result)>0, {
			$array = str.split(input.result, '|')
			if(len(array) == 2, {
				$success_count = 0
				for(x=0, x<sel.count) {
					$new_path = path.combine(path.location(sel[x]), str.replace(path.title(sel[x]), array[0], array[1]) + io.meta(sel[x], 'System.FileExtension'))
					success_count += io.rename(sel[x], new_path) }
				msg('Renamed: ' + success_count + ' files. Errors: ' + (sel.count - success_count)) }, msg('Use format: find|replace . The strings can not be empty')) }, msg('No input')))
	item(title='Find and Replace...' image=\uE8AC tip='Find and replace text in file names' where=keys.shift()
		invoke=1 cmd={
		scs1 = if(cmd.invoked==0, input('Nilesoft Shell', 'Select a string to replace:') and len(input.result)>0, scs1)
		str1 = if(cmd.invoked==0, input.result, str1)
		scs2 = if(cmd.invoked==0, scs1 and input('Nilesoft Shell', 'Select a string to replace with:') and len(input.result)>0, scs2)
		str2 = if(cmd.invoked==0, input.result, str2)
		if(scs1 and scs2, io.rename(sel.path, path.combine(sel.location, str.replace(sel.title, str1, str2) + sel.file.ext))) })
	
	$svg_prefix=image.svg('<svg width="24" height="24" fill="none" viewBox="0 0 24 24">
		<path fill="@image.color2" d="M 6 9 V 6 H 9 V 4 H 6 V 1 H 4 V 4 H 1 V 6 H 4 V 9 H 6 Z" />
		<path fill="@image.color1" d="M 11 4 V 2 H 20 C 21.1 2 22 2.89 22 4 V 7 H 21 C 19 7 17 8 16.999 10.563 S 19 14 21 14 H 22 V 17 C 22 18.1 21.1 19 20 19 H 16.96 H 7 C 5.89 19 5 18.1 5 17 V 11 H 7 V 17 H 12 H 20 V 16 C 16 16 15 13 15.021 10.444 S 16 5 20 5 V 4 H 16"/></svg>')
	$svg_suffix=image.svg('<svg width="24" height="24" fill="none" viewBox="0 0 24 24">
		<path fill="@image.color2" d="M 18 15 V 18 H 15 V 20 H 18 V 23 H 20 V 20 H 23 V 18 H 20 V 15 H 18 Z Z" />
		<path fill="@image.color1" d="M 13 20 V 22 H 4 C 2.9 22 2 21.11 2 20 V 17 H 3 C 5 17 8 16 8.003 13.473 S 5 10 3 10 H 2 V 7 C 2 5.9 2.9 5 4 5 H 7.04 H 17 C 18.11 5 19 5.9 19 7 V 13 H 17 V 7 H 12 V 7 H 4 V 8 C 8 8 10 11 10.008 13.46 S 8 19 4 19 V 20 H 8"/></svg>')
	item(title='Add prefix...' image=svg_prefix
		cmd=if(cmd.invoked || (input('Nilesoft Shell', 'prefix...') and len(input.result)>0), io.rename(sel.path, path.combine(path.location(sel.path), input.result + sel.name)), msg('no input') & break) invoke=1)
	item(title='Add suffix...' image=svg_suffix
		cmd=if(cmd.invoked || (input('Nilesoft Shell', 'suffix...') and len(input.result)>0), io.rename(sel.path, path.combine(path.location(sel.path), path.title(sel.path) + input.result + sel.file.ext)), msg('no input') & break) invoke=1)
	/* those are not implemented yet, because I dont't itend to use them. If you want them, let me know.
		item(title='Remove number of characters from front')
		item(title='Remove number of characters from end')
		item(title='Remove all characters before...')
		item(title='Remove all characters after...')
		item(title='Replace... with...') */
	separator()
	menu(mode='single' type='file' title='Media: mp3' where=sel.file.ext=='.mp3') {
		// Bonus: This is a simple example of how you can rename your music files based on their metadata.
		item(title=io.meta(sel,"System.Music.Artist") + ' - ' + io.meta(sel,"System.Title") + sel.file.ext
			cmd=io.rename(sel.path, path.combine(path.location(sel.path), io.meta(sel,"System.Music.Artist") + ' - ' + io.meta(sel,"System.Title") + sel.file.ext)) invoke=0)
		item(title=io.meta(sel,"System.Music.AlbumTitle") + ' - ' + io.meta(sel,"System.Music.Artist") + ' - ' + io.meta(sel,"System.Title") + sel.file.ext
			cmd=io.rename(sel.path, path.combine(path.location(sel.path), io.meta(sel,"System.Music.AlbumTitle") + ' - ' + io.meta(sel,"System.Music.Artist") + ' - ' + io.meta(sel,"System.Title") + sel.file.ext)) invoke=0) }
}



