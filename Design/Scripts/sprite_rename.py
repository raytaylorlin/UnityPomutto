# filename = 'reimu.png.meta'
# prefix = 'reimu_'
# replace_info = [
#     ['idle_', 0, 10],
#     ['attack1_', 11, 20],
#     ['attack2_', 21, 30],
#     ['hit_', 31, 33],
#     ['down_', 34, 44],
#     ['useitem_', 45, 55],
#     ['spell1_', 56, 67],
#     ['spell2_', 68, 82]
# ]

filename = 'marisa.png.meta'
prefix = 'marisa_'
replace_info = [
    ['idle_', 0, 9],
    ['attack1_', 10, 19],
    ['attack2_', 20, 33],
    ['hit_', 34, 36],
    ['down_', 37, 43],
    ['useitem_', 44, 53],
    ['spell1_', 54, 77],
    ['spell2_', 78, 93]
]


content = open(filename).read()
for info in replace_info:
    replace_str = info[0]
    start = info[1]
    end = info[2]

    cur = 0
    for i in range(start, end + 1):
        target = prefix + str(i) + '\n'
        replace = replace_str + str(cur) + '\n'
        print('Replace {} to {}'.format(target, replace))
        content = content.replace(target, replace)
        cur += 1

result = open(filename + '1', 'w')
result.write(content)
result.close()
