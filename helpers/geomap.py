from pkg_resources import resource_filename

import geopandas


__code_to_state_abbrv = {
        "01": {
            "abbreviation": "AL",
            "name": "Alabama"
        },
        "02": {
            "abbreviation": "AK",
            "name": "Alaska"
        },
        "03": {
            "abbreviation": "AS",
            "name": "American Samoa"
        },
        "04": {
            "abbreviation": "AZ",
            "name": "Arizona"
        },
        "05": {
            "abbreviation": "AR",
            "name": "Arkansas"
        },
        "06": {
            "abbreviation": "CA",
            "name": "California"
        },
        "07": {
            "abbreviation": "CZ",
            "name": "Canal Zone"
        },
        "08": {
            "abbreviation": "CO",
            "name": "Colorado"
        },
        "09": {
            "abbreviation": "CT",
            "name": "Connecticut"
        },
        "10": {
            "abbreviation": "DE",
            "name": "Delaware"
        },
        "11": {
            "abbreviation": "DC",
            "name": "District Of Columbia"
        },
        "12": {
            "abbreviation": "FL",
            "name": "Florida"
        },
        "13": {
            "abbreviation": "GA",
            "name": "Georgia"
        },
        "14": {
            "abbreviation": "GU",
            "name": "Guam"
        },
        "15": {
            "abbreviation": "HI",
            "name": "Hawaii"
        },
        "16": {
            "abbreviation": "ID",
            "name": "Idaho"
        },
        "17": {
            "abbreviation": "IL",
            "name": "Illinois"
        },
        "18": {
            "abbreviation": "IN",
            "name": "Indiana"
        },
        "19": {
            "abbreviation": "IA",
            "name": "Iowa"
        },
        "20": {
            "abbreviation": "KS",
            "name": "Kansas"
        },
        "21": {
            "abbreviation": "KY",
            "name": "Kentucky"
        },
        "22": {
            "abbreviation": "LA",
            "name": "Louisiana"
        },
        "23": {
            "abbreviation": "ME",
            "name": "Maine"
        },
        "24": {
            "abbreviation": "MD",
            "name": "Maryland"
        },
        "25": {
            "abbreviation": "MA",
            "name": "Massachusetts"
        },
        "26": {
            "abbreviation": "MI",
            "name": "Michigan"
        },
        "27": {
            "abbreviation": "MN",
            "name": "Minnesota"
        },
        "28": {
            "abbreviation": "MS",
            "name": "Mississippi"
        },
        "29": {
            "abbreviation": "MO",
            "name": "Missouri"
        },
        "30": {
            "abbreviation": "MT",
            "name": "Montana"
        },
        "31": {
            "abbreviation": "NE",
            "name": "Nebraska"
        },
        "32": {
            "abbreviation": "NV",
            "name": "Nevada"
        },
        "33": {
            "abbreviation": "NH",
            "name": "New Hampshire"
        },
        "34": {
            "abbreviation": "NJ",
            "name": "New Jersey"
        },
        "35": {
            "abbreviation": "NM",
            "name": "New Mexico"
        },
        "36": {
            "abbreviation": "NY",
            "name": "New York"
        },
        "37": {
            "abbreviation": "NC",
            "name": "North Carolina"
        },
        "38": {
            "abbreviation": "ND",
            "name": "North Dakota"
        },
        "39": {
            "abbreviation": "OH",
            "name": "Ohio"
        },
        "40": {
            "abbreviation": "OK",
            "name": "Oklahoma"
        },
        "41": {
            "abbreviation": "OR",
            "name": "Oregon"
        },
        "42": {
            "abbreviation": "PA",
            "name": "Pennsylvania"
        },
        "43": {
            "abbreviation": "PR",
            "name": "Puerto Rico"
        },
        "44": {
            "abbreviation": "RI",
            "name": "Rhode Island"
        },
        "45": {
            "abbreviation": "SC",
            "name": "South Carolina"
        },
        "46": {
            "abbreviation": "SD",
            "name": "South Dakota"
        },
        "47": {
            "abbreviation": "TN",
            "name": "Tennessee"
        },
        "48": {
            "abbreviation": "TX",
            "name": "Texas"
        },
        "49": {
            "abbreviation": "UT",
            "name": "Utah"
        },
        "50": {
            "abbreviation": "VT",
            "name": "Vermont"
        },
        "51": {
            "abbreviation": "VA",
            "name": "Virginia"
        },
        "52": {
            "abbreviation": "VI",
            "name": "Virgin Islands"
        },
        "53": {
            "abbreviation": "WA",
            "name": "Washington"
        },
        "54": {
            "abbreviation": "WV",
            "name": "West Virginia"
        },
        "55": {
            "abbreviation": "WI",
            "name": "Wisconsin"
        },
        "56": {
            "abbreviation": "WY",
            "name": "Wyoming"
        },
        "72": {
            "abbreviation": "PR",
            "name": "Puerto Rico"
        }
    }


def lookup_fips_code(state=None, abbr=False):
    if state is not None:
        for k, v in __code_to_state_abbrv.items():
            abbr_code = v['abbreviation']
            state_name = v['name']
            if abbr is False:
                if state_name == state:
                    return k
            if abbr is True:
                if abbr_code == state:
                    return k
    return None


def lookup_state_names_and_abbreviations():
    """
    Returns a dictionary of supported US States and their Abbreviations
    The abbreviations or state names can be used to lookup FIPS value from the USDA FIPS standard data.
    :return:
    """
    d = dict()
    for k, v in __code_to_state_abbrv.items():
        d[v['name']] = v['abbreviation']
    return d


def get_world_data():
    world = geopandas.read_file(geopandas.datasets.get_path('naturalearth_lowres'))
    return world


def get_usa_state_data(mainland_only=True):
    """
    cb_2018_us_state_500k is the US Census Obtained Shape File Data.
    :param mainland_only:
    :return: dataframe of states
    """
    usa_shape_file = resource_filename(__name__, 'cb_2018_us_state_500k/cb_2018_us_state_500k.shp')
    states = geopandas.read_file(usa_shape_file)
    possible_drop_list = ['AK', 'HI', 'PR', 'RI', 'VI', 'AS', 'GU', 'MP']
    if mainland_only:
        mainland_usa = states[~states.STUSPS.isin(possible_drop_list)]
        mainland_usa = mainland_usa.reset_index()
        return mainland_usa
    # Otherwise, return the data ready for an inset graph
    return states


def get_usa_county_level_data(state_fips_code=None):
    usa_county_file = resource_filename(__name__, 'cb_2018_us_county_500k/cb_2018_us_county_500k.shp')
    states_and_counties = geopandas.read_file(usa_county_file)
    if state_fips_code is None:
        return states_and_counties
    filtered_data = states_and_counties[states_and_counties['STATEFP'] == state_fips_code]
    filtered_data = filtered_data.reset_index()
    return filtered_data