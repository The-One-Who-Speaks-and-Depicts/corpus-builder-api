# -*- coding: utf-8 -*-

import re

def analysis(processed_string, stop_symbols, decapitalization):    
    paragraphs = re.split('\n|\r',processed_string)
    paragraphs = [p for p in paragraphs if p != '']
    words_by_paragraphs = []
    for p in paragraphs:
        current_words = p.split(' ')
        prepared_words = []
        for w in current_words:
          lexeme2 = w
          for s in stop_symbols:
            lexeme2 = lexeme2.replace(s, "")
          if (decapitalization == "True"):
            lexeme2 = lexeme2.lower()
          graphemes = list(w)
          prepared_words.append([w, lexeme2, graphemes])
        words_by_paragraphs.append([p, prepared_words])    
    return words_by_paragraphs
