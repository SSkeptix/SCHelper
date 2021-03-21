module Program




type Modification =
    | Damage of float
    | FireRate of float

type Ship = {
    Name: string;
    Level: int;
    WeaponCount: int;
    MaxChipCount: int;
    Bonuces: Modification list
}