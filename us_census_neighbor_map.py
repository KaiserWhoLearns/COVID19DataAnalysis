import json
from collections import defaultdict

from data import loader

__RELATED_NEIGHBOR_PREFIX = ['', '']

# Maintain state wise indexing
main_county_mapping = defaultdict(list)
# Internally maintains county wise indexing of all counties


def split_county_name_to_state_segments(full_county_name):
    state = full_county_name[-2:]
    if state == "LA":
        return [
            'LA',
            full_county_name.replace(" Parish", "")[0:-4]
        ]
    elif state == "AK":
        return ['AK',
                full_county_name.replace(" City and Borough", "")
                    .replace(" Census Area", "")
                    .replace(" Municipality", "")
                    .replace(" Borough", "")[0:-4]
                ]
    else:
        return [state,
                full_county_name.replace(" County", "")[0:-4]
                ]


def process_child_entry(child_entry):
    data_considered = child_entry[len(__RELATED_NEIGHBOR_PREFIX):]
    county_state, county_name = split_county_name_to_state_segments(data_considered[0][1:len(data_considered[0]) - 1])
    county_fips_full = data_considered[1].strip('\n')
    return county_state, county_name, county_fips_full


def process_parent_entry(parent_entry):
    data_considered = parent_entry
    main_county_state_location, main_county_name = split_county_name_to_state_segments(parent_entry[0][1:len(data_considered[0]) - 1])
    main_county_fips = data_considered[1]
    return main_county_name, main_county_state_location, main_county_fips


def process_line_entry(line):
    s = line.split('\t')
    if s[:len(__RELATED_NEIGHBOR_PREFIX)] == __RELATED_NEIGHBOR_PREFIX:
        child = True
        county_state, county_name, county_fips_full = process_child_entry(s)
        # print('\t', county_state, county_name, county_fips_full)
        return child, county_state, county_name, county_fips_full
    else:
        child = False
        county_name, county_state, county_fips_full = process_parent_entry(s)
        # main_county_mapping[county_state].append(county_name)
        # print(county_state, county_name, county_fips_full)
        return child, county_state, county_name, county_fips_full


if __name__ == '__main__':
    l = loader.generate_neighbor_map_from_census_data()
    t = {}
    prev = 'first'
    main = None
    main_county_state = None
    main_county_fips = None
    for line in l:
        is_child, county_state, county_name, county_fips_full = process_line_entry(line)
        if is_child == False and prev == 'second':
            main_county_mapping[main_county_state].append(t)
            t = {}
            prev = 'first'
            main = None
        if is_child == False and prev == 'first':
            main = county_name
            main_county_state = county_state
            main_county_fips = county_fips_full
            t[f'{main} ({main_county_fips})'] = []
            prev = 'second'
        if is_child == True:
            d = t[f'{main} ({main_county_fips})']
            r = {}
            r['county'] = county_name
            r['state'] = county_state
            r['fips'] = county_fips_full
            d.append(r)
    # print(main_county_mapping)

    with open('data/neighbor_map/USA/all.json', 'w') as f:
        json_string = json.dumps(main_county_mapping, indent=4)
        f.write(json_string)
