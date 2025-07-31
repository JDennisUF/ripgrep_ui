window.fileSystemHelpers = {
    showDirectoryPicker: async function () {
        try {
            // Check if the browser supports the File System Access API
            if ('showDirectoryPicker' in window) {
                const dirHandle = await window.showDirectoryPicker();
                return dirHandle.name;
            } else {
                // Fallback for browsers that don't support File System Access API
                const input = document.createElement('input');
                input.type = 'file';
                input.webkitdirectory = true;
                input.multiple = true;
                
                return new Promise((resolve) => {
                    input.onchange = function(e) {
                        if (e.target.files.length > 0) {
                            // Get the directory path from the first file
                            const firstFile = e.target.files[0];
                            const fullPath = firstFile.webkitRelativePath;
                            const dirPath = fullPath.substring(0, fullPath.lastIndexOf('/'));
                            resolve(dirPath || firstFile.name.split('/')[0]);
                        } else {
                            resolve(null);
                        }
                    };
                    input.click();
                });
            }
        } catch (error) {
            console.error('Directory picker error:', error);
            return null;
        }
    },

    // Alternative method using prompt for manual entry with common paths
    promptForDirectory: function() {
        const commonPaths = [
            'C:\\Users\\' + (process.env.USERNAME || 'YourUsername'),
            'C:\\Projects',
            'C:\\Code',
            'C:\\Source',
            'D:\\',
            'Documents',
            'Desktop'
        ];
        
        let message = 'Enter directory path:\n\nCommon paths:\n';
        message += commonPaths.join('\n');
        
        const path = prompt(message, 'C:\\');
        return path;
    },

    // Get user profile directory
    getUserProfilePath: function() {
        // This will be handled server-side, but we can provide a client-side fallback
        return 'C:\\Users\\' + (window.navigator.userAgent.includes('Windows') ? 'YourUsername' : 'User');
    },

    // Validate path format (basic Windows path validation)
    isValidWindowsPath: function(path) {
        if (!path) return false;
        
        // Basic Windows path validation
        const windowsPathRegex = /^[a-zA-Z]:\\(?:[^<>:"/\\|?*]+\\)*[^<>:"/\\|?*]*$/;
        const uncPathRegex = /^\\\\[^<>:"/\\|?*]+\\[^<>:"/\\|?*]+(?:\\[^<>:"/\\|?*]+)*$/;
        
        return windowsPathRegex.test(path) || uncPathRegex.test(path);
    }
};