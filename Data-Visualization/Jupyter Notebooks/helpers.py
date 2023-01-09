class ftxt:
    """
    Provides strings for coloring/formatting text in the terminal.

    Colors: RED, GREEN, BLUE, YELLOW, PURPLE, CYAN.
        other Attributes:
            END (str): end the string with this to end text formatting
            BOLD (str): for writing bold text
            UNDERLINE (str): for writing underlined text        
    """

    RED = '\033[91m'
    GREEN = '\033[92m'
    BLUE = '\033[94m'
    YELLOW = '\033[93m'
    PURPLE = '\033[95m'
    CYAN = '\033[96m'
    END = '\033[0m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'