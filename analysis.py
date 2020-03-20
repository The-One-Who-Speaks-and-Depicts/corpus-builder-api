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
    verb_endings = ['ûtь$', 'eli$', 'ili$', 'ite$', 'etь$', 'utь$', 'ete$', 'itь$', 'ti$', 'ate$', 'ĉi$']
    irregular_verbs = ['e']
    pre_tagged = [('vasʼpetь', 'ADVB')]
    normal_length = 3
    tagged_by_paragraphs = []
    for paragraph in words_by_paragraphs:
      tagged_by_paragraphs.append([])
      for word in paragraph:    
        word_tagged = False
        for unit in pre_tagged:
          if (word == unit[0]):
            tagged_by_paragraphs[len(tagged_by_paragraphs) - 1].append((word, unit[1]))
            word_tagged = True
            break
        for ending in verb_endings:
          if (word_tagged):
            break
          if re.search(ending, word) and len(word) > normal_length:
            tagged_by_paragraphs[len(tagged_by_paragraphs) - 1].append((word, 'V'))
            word_tagged = True
            break
        for form in irregular_verbs:
          if (word_tagged):
            break
          if (word == form):
            tagged_by_paragraphs[len(tagged_by_paragraphs) - 1].append((word, 'V'))
            word_tagged = True
            break
        if (not word_tagged):
          tagged_by_paragraphs[len(tagged_by_paragraphs) - 1].append((word, 'N'))
    tagged_by_paragraphs_alphabetically = []
    for paragraph in tagged_by_paragraphs:
      tagged_by_paragraphs_alphabetically.append([])
      for word in paragraph:
        tagged_by_paragraphs_alphabetically[len(tagged_by_paragraphs_alphabetically) - 1].append((word[0].lower(), word[1]))
    tagged_by_paragraphs_alphabetically = sorted(tagged_by_paragraphs_alphabetically)
    with io.open('D:\\result.txt', 'w', 1, encoding='utf-8') as out:
        for paragraph in tagged_by_paragraphs_alphabetically:
            for word in paragraph:
                #do something
                out.write(word[0])
    #and do normal output, lol
    return normal_length
