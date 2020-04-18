# -*- coding: utf-8 -*-

import re
import sys, argparse
import io

def analysis(processed_string):
    string = processed_string
    paragraphs = string.split('\n')
    words_by_paragraphs = []
    for p in paragraphs:
        words_by_paragraphs.append(p.split(' '))    
    #tagged_by_paragraphs = []
    #for paragraph in words_by_paragraphs:
      #tagged_by_paragraphs.append([])
      #for word in paragraph:
           #processed_word = word.split('{')
           #tags = processed_word[1][0:len(processed_word[1]) - 1]
           #tags = re.sub("PoS=", "", tags)
           #tagged_by_paragraphs[len(tagged_by_paragraphs) - 1].append((processed_word[0], tags))
    words_by_paragraphs_alphabetically = []
    for paragraph in words_by_paragraphs:
      for word in paragraph:
        words_by_paragraphs_alphabetically.append(word.lower())
    words_by_paragraphs_alphabetically = sorted(words_by_paragraphs_alphabetically)
    return [paragraphs, words_by_paragraphs, words_by_paragraphs_alphabetically]
