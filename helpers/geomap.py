from pkg_resources import resource_filename

import geopandas


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
        return mainland_usa
    # Otherwise, return the data ready for an inset graph
    return states
